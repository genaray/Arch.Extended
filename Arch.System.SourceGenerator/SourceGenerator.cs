using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arch.System.SourceGenerator;

[Generator]
public class QueryGenerator : IIncrementalGenerator
{
    private static Dictionary<ISymbol, List<IMethodSymbol>> _classToMethods { get; set; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _classToMethods = new(512);
        //if (!Debugger.IsAttached) Debugger.Launch();

        // Register the generic attributes 
        var attributes = $$"""
            namespace Arch.System.SourceGenerator
            {
                {{new StringBuilder().AppendGenericAttributes("All", "All", 25)}}
                {{new StringBuilder().AppendGenericAttributes("Any", "Any", 25)}}
                {{new StringBuilder().AppendGenericAttributes("None", "None", 25)}}
                {{new StringBuilder().AppendGenericAttributes("Exclusive", "Exclusive", 25)}}
            }
        """;
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("Attributes.g.cs", SourceText.From(attributes, Encoding.UTF8)));
        
        // Do a simple filter for methods marked with update
        IncrementalValuesProvider<MethodDeclarationSyntax> methodDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                 static (s, _) => s is MethodDeclarationSyntax { AttributeLists.Count: > 0 },
                 static (ctx, _) => GetMethodSymbolIfAttributeof(ctx, "Arch.System.SourceGenerator.UpdateAttribute")
        ).Where(static m => m is not null)!; // filter out attributed methods that we don't care about

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndMethods, static (spc, source) => Generate(source.Item1, source.Item2, spc));
    }
    
    /// <summary>
    ///     Adds a <see cref="IMethodSymbol"/> to its class.
    ///     Stores them in <see cref="_classToMethods"/>.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> which will be added/mapped to its class.</param>
    private static void AddMethodToClass(IMethodSymbol methodSymbol)
    {
        if (!_classToMethods.TryGetValue(methodSymbol.ContainingSymbol, out var list))
        {
            list = new List<IMethodSymbol>();
            _classToMethods[methodSymbol.ContainingSymbol] = list;
        }
        list.Add(methodSymbol);
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
        var enumDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) continue;
                
                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName != name) continue;
                return enumDeclarationSyntax;
            }
        }

        // we didn't find the attribute we were looking for
        return null;
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
        
        // Creating methods
        var distinctEnums = methods.Distinct();
        foreach (var methodSyntax in distinctEnums)
        {
            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var methodSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, methodSyntax) as IMethodSymbol;

            AddMethodToClass(methodSymbol);

            var entity = methodSymbol.Parameters.Any(symbol => symbol.Type.Name.Equals("Entity"));
            var sb = new StringBuilder();
            var method = entity ? sb.AppendQueryWithEntity(methodSymbol) : sb.AppendQueryWithoutEntity(methodSymbol);
            context.AddSource($"{methodSymbol.Name}.g.cs",  CSharpSyntaxTree.ParseText(method.ToString()).GetRoot().NormalizeWhitespace().ToFullString());
        }

        // Creating class that calls the created methods after another.
        foreach (var classToMethod in _classToMethods)
        {
            
            // Get BaseSystem class
            var classSymbol = classToMethod.Key as INamedTypeSymbol;
            var parentSymbol = classSymbol.BaseType;

            if(!parentSymbol.Name.Equals("BaseSystem")) continue;     // Ignore classes which do not derive from BaseSystem
            if(classSymbol.MemberNames.Contains("Update")) continue;  // Update was implemented by user, no need to do that by source generator. 
            
            // Get generic of BaseSystem
            var typeSymbol = parentSymbol.TypeArguments[1];
            
            var methodCalls = new StringBuilder().CallMethods(classToMethod.Value);
            var template = 
            $$"""
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using {{typeSymbol.ContainingNamespace}};
            namespace {{classSymbol.ContainingNamespace}}{
                public partial class {{classSymbol.Name}}{
                        
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public override void Update(in {{typeSymbol.Name}} {{typeSymbol.Name.ToLower()}}){
                        {{methodCalls}}
                    }
                }
            }
            """;
            
            context.AddSource($"{classSymbol.Name}.g.cs",  CSharpSyntaxTree.ParseText(template).GetRoot().NormalizeWhitespace().ToFullString());
        }
    }
}