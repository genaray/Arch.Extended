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
    private static Dictionary<ISymbol, List<string>> _classToMethods { get; set; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _classToMethods = new(512);
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }

        var attributes = $$"""
            namespace Arch.System.SourceGenerator
            {
                {{new StringBuilder().AppendGenericAttributes("All", "All", 25)}}
                {{new StringBuilder().AppendGenericAttributes("Any", "Any", 25)}}
                {{new StringBuilder().AppendGenericAttributes("None", "None", 25)}}
                {{new StringBuilder().AppendGenericAttributes("Exclusive", "Exclusive", 25)}}
            }
        """;
        
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "Attributes.g.cs", 
                SourceText.From(attributes, Encoding.UTF8)
            )
        );
        
        // Do a simple filter for enums
        IncrementalValuesProvider<MethodDeclarationSyntax> methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is MethodDeclarationSyntax { AttributeLists.Count: > 0 }, // select methods with attributes
                transform: static (ctx, _) => GetIfMethodHasAttributeOf(ctx, "Arch.System.SourceGenerator.UpdateAttribute")) // sect the enum with the [EnumExtensions] attribute
            .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndMethods, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static void Add(IMethodSymbol methodSymbol)
    {
        if (!_classToMethods.TryGetValue(methodSymbol.ContainingSymbol, out var list))
        {
            list = new List<string>();
            _classToMethods[methodSymbol.ContainingSymbol] = list;
        }
        list.Add(methodSymbol.Name+"Query");
    }
    
    static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
    {
        if (methods.IsDefaultOrEmpty) return;
        
        // Creating methods
        var distinctEnums = methods.Distinct();
        foreach (var methodSyntax in distinctEnums)
        {
            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var methodSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, methodSyntax) as IMethodSymbol;

            Add(methodSymbol);

            var entity = methodSymbol.Parameters.Any(symbol => symbol.Type.Name.Equals("Entity"));
            var sb = new StringBuilder();
            var method = entity ? sb.AppendQueryWithEntity(methodSymbol) : sb.AppendQueryWithoutEntity(methodSymbol);
            context.AddSource($"{methodSymbol.Name}.g.cs",  CSharpSyntaxTree.ParseText(method.ToString()).GetRoot().NormalizeWhitespace().ToFullString());
        }

        // Creating class that calls the created methods after another.
        foreach (var classToMethod in _classToMethods)
        {
            var classSymbol = classToMethod.Key as INamedTypeSymbol;
            if(classSymbol.MemberNames.Contains("Update")) continue;  // Update was implemented by user, no need to do that by source generator. 
            
            var parentSymbol = classSymbol.BaseType;
            var typeSymbol = parentSymbol.TypeArguments[1];
            
            var methodCalls = new StringBuilder().GetMethods(classToMethod.Value);
            var template = 
            $$"""
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using {{typeSymbol.ContainingNamespace}};
            namespace {{classSymbol.ContainingNamespace}};
            public partial class {{classSymbol.Name}}{
                    
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public override void Update(in {{typeSymbol.Name}} {{typeSymbol.Name.ToLower()}}){
                    {{methodCalls}}
                }
            }
            """;
            
            context.AddSource($"{classSymbol.Name}.g.cs",  CSharpSyntaxTree.ParseText(template).GetRoot().NormalizeWhitespace().ToFullString());
        }
    }
    
    static MethodDeclarationSyntax? GetIfMethodHasAttributeOf(GeneratorSyntaxContext context, string name)
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
    
    
}