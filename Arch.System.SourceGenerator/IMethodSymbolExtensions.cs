using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class IMethodSymbolExtensions
{
    public static INamedTypeSymbol GetAttribute(this IMethodSymbol ms, string name)
    {
        foreach (var attribute in ms.GetAttributes())
        {
            var classSymbol = attribute.AttributeClass;
            if(!classSymbol.Name.Contains(name)) continue;

            return classSymbol;
        }

        return default;
    }
}