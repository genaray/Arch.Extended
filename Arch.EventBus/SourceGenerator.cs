using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arch.Bus;

[Generator]
public class QueryGenerator : IIncrementalGenerator
{
    private static EventBus _eventBus;
    private static Hooks _hooks;
    
    private static Dictionary<ITypeSymbol, (RefKind, IList<ReceivingMethod>)> _eventTypeToReceivingMethods;
    private static Dictionary<ITypeSymbol, IList<EventHook>> _containingTypeToReceivingMethods;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached) Debugger.Launch();

        // Register the generic attributes 
        var attributes = $$"""
            namespace Arch.Bus
            {           
                /// <summary>
                ///     Marks a method to receive a certain event. 
                /// </summary>
                [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
                public class EventAttribute : global::System.Attribute
                {
                    public EventAttribute(int order = 0)
                    {
                        Order = order;
                    }

                    /// <summary>
                    /// The order of this event. 
                    /// </summary>
                    public int Order { get; }
                }
            }
        """;
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("Attributes.g.cs", SourceText.From(attributes, Encoding.UTF8)));

        // Do a simple filter for methods marked with update
        IncrementalValuesProvider<MethodDeclarationSyntax> methodDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            static (s, _) => s is MethodDeclarationSyntax { AttributeLists.Count: > 0 },
            static (ctx, _) => GetMethodSymbolIfAttributeof(ctx, "Arch.Bus.EventAttribute")
        ).Where(static m => m is not null)!; // filter out attributed methods that we don't care about
        
        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods =
            context.CompilationProvider.Combine(methodDeclarations.WithComparer(Comparer.Instance).Collect());
        context.RegisterSourceOutput(compilationAndMethods, static (spc, source) => Generate(source.Item1, source.Item2, spc));
    }
    

    /// <summary>
    ///     Returns a <see cref="MethodDeclarationSyntax"/> if its annocated with a attribute of <see cref="name"/>.
    /// </summary>
    /// <param name="context">Its <see cref="GeneratorSyntaxContext"/>.</param>
    /// <param name="name">The attributes name.</param>
    /// <returns></returns>
    private static MethodDeclarationSyntax? GetMethodSymbolIfAttributeof(GeneratorSyntaxContext context, string name)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (var attributeListSyntax in methodDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName != name) continue;
                return methodDeclarationSyntax;
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    /// <summary>
    ///     Maps the <see cref="IMethodSymbol"/> to its <see cref="IParameterSymbol"/> for organisation. 
    /// </summary>
    /// <param name="methodSymbol"></param>
    private static void MapMethodToEventType(IMethodSymbol methodSymbol)
    {
        var eventType = methodSymbol.Parameters[0];
        var param = methodSymbol.GetAttributes()[0].ConstructorArguments[0];
        var receivingMethod = new ReceivingMethod{ Static = methodSymbol.IsStatic, MethodSymbol = methodSymbol, Order = (int)param.Value };

        // Either append or create a new receiving method with a new list.
        if (_eventTypeToReceivingMethods.TryGetValue(eventType.Type, out var tuple))
        {
            tuple.Item2.Add(receivingMethod);
        }
        else
        {
            tuple.Item1 = eventType.RefKind;
            tuple.Item2 = new List<ReceivingMethod>{receivingMethod};
            _eventTypeToReceivingMethods.Add(eventType.Type, tuple);
        }
    }
    
    /// <summary>
    ///     Maps the <see cref="IMethodSymbol"/> to its <see cref="ITypeSymbol"/> for organisation. 
    /// </summary>
    /// <param name="methodSymbol"></param>
    private static void MapMethodToContainingType(IMethodSymbol methodSymbol)
    {
        var eventType = methodSymbol.Parameters[0];
        var receivingMethod = new EventHook{ EventType = eventType.Type, MethodSymbol = methodSymbol, };

        // Either append or create a new receiving method with a new list.
        if (_containingTypeToReceivingMethods.TryGetValue(methodSymbol.ContainingType, out var tuple))
        {
            tuple.Add(receivingMethod);
        }
        else
        {
            var list = new List<EventHook>{receivingMethod};
            _containingTypeToReceivingMethods.Add(methodSymbol.ContainingType, list);
        }
    }
    
    /// <summary>
    ///     Prepares the <see cref="EventBus"/> by convertings the <see cref="_eventTypeToReceivingMethods"/> to the eventbus model. 
    /// </summary>
    private static void PrepareEventBus()
    {
        // Translate mapping to the model
        foreach (var kvp in _eventTypeToReceivingMethods)
        {
            var eventCallMethod = new Method
            {
                RefKind = kvp.Value.Item1,
                EventType = kvp.Key,
                EventReceivingMethods = kvp.Value.Item2
            };
            eventCallMethod.EventReceivingMethods = eventCallMethod.EventReceivingMethods.OrderBy(method => method.Order).ToList();
            _eventBus.Methods.Add(eventCallMethod);
        }
    }
    
    /// <summary>
    ///     Prepares the <see cref="Hooks"/> by convertings the <see cref="_containingTypeToReceivingMethods"/> to the hooks model. 
    /// </summary>
    private static void PrepareHooks()
    {
        // Translate mapping to the model
        foreach (var kvp in _containingTypeToReceivingMethods)
        {

            // Skip static classes since they need no hooks
            if (kvp.Key.IsStatic)
            {
                continue;
            }
            
            var hook = new ClassHooks
            {
                PartialClass = kvp.Key,
                EventHooks = kvp.Value
            };
            _hooks.Instances.Add(hook);
        }
    }

    /// <summary>
    ///     Generates queries and partial classes for the found marked methods.
    /// </summary>
    /// <param name="compilation">The <see cref="Compilation"/>.</param>
    /// <param name="methods">The <see cref="ImmutableArray{MethodDeclarationSyntax}"/> array, the methods which we will generate queries and classes for.</param>
    /// <param name="context">The <see cref="SourceProductionContext"/>.</param>
    private static void Generate(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
    {
        if (methods.IsDefaultOrEmpty) return;
        
        // Init 
        _eventBus.Namespace = "Arch.Bus";
        _eventBus.Methods = new List<Method>(512);
        _eventTypeToReceivingMethods = new Dictionary<ITypeSymbol, (RefKind, IList<ReceivingMethod>)>(512);
        
        _hooks.Instances = new List<ClassHooks>(512);
        _containingTypeToReceivingMethods = new Dictionary<ITypeSymbol, IList<EventHook>>(512);
        
        // Generate Query methods and map them to their classes
        foreach (var methodSyntax in methods)
        {
            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var methodSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, methodSyntax) as IMethodSymbol;

            MapMethodToEventType(methodSymbol);
            MapMethodToContainingType(methodSymbol);
        }
        
        PrepareEventBus();
        PrepareHooks();
        
        var template = new StringBuilder().AppendEventBus(ref _eventBus);
        var hooks = new StringBuilder().AppendHooks(ref _hooks);
        
        context.AddSource($"EventBus.g.cs", CSharpSyntaxTree.ParseText(template.ToString()).GetRoot().NormalizeWhitespace().ToFullString());
        context.AddSource($"Hooks.g.cs", CSharpSyntaxTree.ParseText(hooks.ToString()).GetRoot().NormalizeWhitespace().ToFullString());
    }

    /// <summary>
    /// Compares <see cref="MethodDeclarationSyntax"/>s to remove duplicates. 
    /// </summary>
    class Comparer : IEqualityComparer<MethodDeclarationSyntax>
    {
        public static readonly Comparer Instance = new Comparer();

        public bool Equals(MethodDeclarationSyntax x, MethodDeclarationSyntax y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(MethodDeclarationSyntax obj)
        {
            return obj.GetHashCode();
        }
    }
}