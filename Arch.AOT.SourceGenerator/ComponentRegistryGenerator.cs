using System.Collections.Immutable;
using System.Text;
using Arch.SourceGen.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Arch.SourceGen;

/// <summary>
///     Incremental generator that generates a class that adds all components to the ComponentRegistry.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ComponentRegistryGenerator : IIncrementalGenerator
{
	private readonly List<ComponentType> componentTypes = new List<ComponentType>();
	private const string ATTRIBUTE_TEMPLATE = @"using System;

namespace Arch.Core
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ComponentAttribute : Attribute { }
}";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Register the attribute.
		context.RegisterPostInitializationOutput(initializationContext =>
		{
			initializationContext.AddSource("Components.Attributes.g.cs", SourceText.From(ATTRIBUTE_TEMPLATE, Encoding.UTF8));
		});

		IncrementalValuesProvider<TypeDeclarationSyntax> provider = context.SyntaxProvider.CreateSyntaxProvider(
			ShouldTypeBeRegistered,
			GetMemberDeclarationsForSourceGen).Where(t => t.attributeFound).Select((t, _) => t.Item1);

		context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
			(productionContext, tuple) => GenerateCode(productionContext, tuple.Left, tuple.Right));
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
	private static (TypeDeclarationSyntax, bool attributeFound) GetMemberDeclarationsForSourceGen(GeneratorSyntaxContext context,
		CancellationToken cancellationToken)
	{
		TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax) context.Node;

		// Stop here if we can't get the type symbol for some reason.
		if (context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not ITypeSymbol symbol)
		{
			return (typeDeclarationSyntax, false);
		}

		// Go through all the attributes.
		foreach (AttributeData? attributeData in symbol.GetAttributes())
		{
			if (attributeData.AttributeClass is null)
			{
				continue;
			}

			// If the attribute is the Component attribute, we can stop here and return true.
			if (string.Equals(attributeData.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), "global::Arch.Core.ComponentAttribute",
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
		StringBuilder sb = new StringBuilder();
		componentTypes.Clear();

		foreach (TypeDeclarationSyntax? type in typeList)
		{
			// Get the symbol for the type.
			ISymbol? symbol = compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type);

			// If the symbol is not a type symbol, we can't do anything with it.
			if (symbol is not ITypeSymbol typeSymbol)
			{
				continue;
			}

			// Check if there are any fields in the type.
			bool hasZeroFields = true;
			foreach (ISymbol? member in typeSymbol.GetMembers())
			{
				if (member is IFieldSymbol)
				{
					hasZeroFields = false;
					break;
				}
			}

			string typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

			componentTypes.Add(new ComponentType(typeName, hasZeroFields, typeSymbol.IsValueType));
		}

		sb.AppendComponentTypes(componentTypes);

		productionContext.AddSource("GeneratedComponentRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
	}
}