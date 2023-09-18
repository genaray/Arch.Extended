using Microsoft.CodeAnalysis;

namespace Arch.Bus;

public static class IMethodSymbolExtensions
{

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
    
    /// <summary>
    ///     Gets all the types of a <see cref="AttributeData"/> as <see cref="ITypeSymbol"/>s and adds them to a list.
    ///     If the attribute is generic it will add the generic parameters, if its non generic it will add the non generic types from the constructor.
    /// </summary>
    /// <param name="data">The <see cref="AttributeData"/>.</param>
    /// <param name="array">The <see cref="List{T}"/> where the found <see cref="ITypeSymbol"/>s are added to.</param>
    public static void GetAttributeTypes(this AttributeData data, List<ITypeSymbol> array)
    {
        if (data is not null && data.AttributeClass.IsGenericType)
        {
            array.AddRange(data.AttributeClass.TypeArguments);
        }
        else if (data is not null && !data.AttributeClass.IsGenericType)
        {
            var constructorArguments = data.ConstructorArguments[0].Values;
            var constructorArgumentsTypes = constructorArguments.Select(constant => constant.Value as ITypeSymbol).ToList();
            array.AddRange(constructorArgumentsTypes);
        }
    }
}