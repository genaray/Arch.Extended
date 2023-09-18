using System.Collections.Immutable;
using System.Text;
using Arch.AOT.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arch.AOT.SourceGenerator;

/// <summary>
///     Incremental generator that generates a class that adds all components to the ComponentRegistry.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ComponentRegistryGenerator : IIncrementalGenerator
{
	/// <summary>
	///		A <see cref="List{T}"/> of annotated components (their types) found via the source-gen. 
	/// </summary>
	private readonly List<ComponentType> _componentTypes = new();
	
	/// <summary>
	///		The attribute to mark components with in order to be found by this source-gen. 
	/// </summary>
	private const string AttributeTemplate = """
	                                         using System;

	                                         namespace Arch.AOT.SourceGenerator
	                                         {
	                                             [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	                                             public sealed class ComponentAttribute : Attribute { }
	                                         }
	                                         """;
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Register the attribute.
		context.RegisterPostInitializationOutput(initializationContext =>
		{
			initializationContext.AddSource("Components.Attributes.g.cs", SourceText.From(AttributeTemplate, Encoding.UTF8));
		});

		var provider = context.SyntaxProvider.CreateSyntaxProvider(
			ShouldTypeBeRegistered,
			GetMemberDeclarationsForSourceGen).Where(t => t.attributeFound).Select((t, _) => t.Item1
		);

		context.RegisterSourceOutput(
			context.CompilationProvider.Combine(provider.Collect()), (productionContext, tuple) => GenerateCode(productionContext, tuple.Left, tuple.Right)
		);
	}

	/// <summary>
	///     Determines if a node should be considered for code generation.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	private static bool ShouldTypeBeRegistered(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node is not TypeDeclarationSyntax typeDeclarationSyntax)
		{
			return false;
		}

		return typeDeclarationSyntax.AttributeLists.Count != 0;
	}

	/// <summary>
	///     Make sure the type is annotated with the Component attribute.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	private static (TypeDeclarationSyntax, bool attributeFound) GetMemberDeclarationsForSourceGen(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var typeDeclarationSyntax = (TypeDeclarationSyntax) context.Node;

		// Stop here if we can't get the type symbol for some reason.
		if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, typeDeclarationSyntax) is not ITypeSymbol symbol)
		{
			return (typeDeclarationSyntax, false);
		}

		// Go through all the attributes.
		foreach (var attributeData in symbol.GetAttributes())
		{
			if (attributeData.AttributeClass is null)
			{
				continue;
			}

			// If the attribute is the Component attribute, we can stop here and return true.
			if (string.Equals(attributeData.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), "global::Arch.AOT.SourceGenerator.ComponentAttribute",
				    StringComparison.Ordinal))
			{
				return (typeDeclarationSyntax, true);
			}
		}

		// No attribute found, return false.
		return (typeDeclarationSyntax, false);
	}

	private void GenerateCode(SourceProductionContext productionContext, Compilation compilation, ImmutableArray<TypeDeclarationSyntax> typeList)
	{
		var sb = new StringBuilder();
		_componentTypes.Clear();

		foreach (var type in typeList)
		{
			// Get the symbol for the type.
			var symbol = ModelExtensions.GetDeclaredSymbol(compilation.GetSemanticModel(type.SyntaxTree), type);

			// If the symbol is not a type symbol, we can't do anything with it.
			if (symbol is not ITypeSymbol typeSymbol)
			{
				continue;
			}

			// Check if there are any fields in the type.
			var hasZeroFields = true;
			foreach (var member in typeSymbol.GetMembers())
			{
				if (member is not IFieldSymbol) continue;
				
				hasZeroFields = false;
				break;
			}

			var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			_componentTypes.Add(new ComponentType(typeName, hasZeroFields, typeSymbol.IsValueType));
		}

		sb.AppendComponentTypes(_componentTypes);
		productionContext.AddSource("GeneratedComponentRegistry.g.cs",CSharpSyntaxTree.ParseText(sb.ToString()).GetRoot().NormalizeWhitespace().ToFullString());
	}
}