using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class IMethodSymbolExtensions
{
    /// <summary>
    ///     Searches attributes of a <see cref="IMethodSymbol"/> and returns the first one found.
    /// </summary>
    /// <param name="ms">The <see cref="IMethodSymbol"/> instance.</param>
    /// <param name="name">The attributes name.</param>
    /// <returns>The attribute wrapped in an <see cref="INamedTypeSymbol"/>.</returns>
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
    
    /// <summary>
    ///     Searches attributes of a <see cref="IMethodSymbol"/> and returns the first one found.
    /// </summary>
    /// <param name="ms">The <see cref="IMethodSymbol"/> instance.</param>
    /// <param name="name">The attributes name.</param>
    /// <returns>The attribute wrapped in an <see cref="AttributeData"/>.</returns>
    public static AttributeData GetAttributeData(this IMethodSymbol ms, string name)
    {
        foreach (var attribute in ms.GetAttributes())
        {
            var classSymbol = attribute.AttributeClass;
            if(!classSymbol.Name.Contains(name)) continue;

            return attribute;
        }

        return default;
    }
    
}