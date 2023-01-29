using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class QueryUtils
{
    
    /// <summary>
    ///     Appends the arrays of the types specified in the <see cref="parameterSymbols"/> from the chunk.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> list of <see cref="IParameterSymbol"/>s which we wanna append the arrays for.</param>
    /// <returns></returns>
    public static StringBuilder GetArrays(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity" || !symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data"))) // Prevent entity being added to the type array
                sb.AppendLine($"var {symbol.Type.Name.ToLower()}Array = chunk.GetArray<{symbol.Type}>();");

        return sb;
    }
    
    /// <summary>
    ///     Appends the first elements of the types specified in the <see cref="parameterSymbols"/> from the previous specified arrays.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> list of <see cref="IParameterSymbol"/>s which we wanna append the first elements for.</param>
    /// <returns></returns>
    public static StringBuilder GetFirstElements(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
      
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Type.Name.ToLower()}FirstElement = ref ArrayExtensions.DangerousGetReference({symbol.Type.Name.ToLower()}Array);");

        return sb;
    }
    
    /// <summary>
    ///     Appends the components of the types specified in the <see cref="parameterSymbols"/> from the previous specified first elements.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> list of <see cref="IParameterSymbol"/>s which we wanna append the components for.</param>
    /// <returns></returns>
    public static StringBuilder GetComponents(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Name.ToLower()} = ref Unsafe.Add(ref {symbol.Type.Name.ToLower()}FirstElement, entityIndex);");

        return sb;
    }
    
    /// <summary>
    ///     Inserts the types defined in the <see cref="parameterSymbols"/> as parameters in a method.
    ///     <example>ref position, out velocity,...</example>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> of <see cref="IParameterSymbol"/>s which we wanna insert.</param>
    /// <returns></returns>
    public static StringBuilder InsertParams(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            sb.Append($"{CommonUtils.RefKindToString(symbol.RefKind)} {symbol.Name.ToLower()},");
        
        if(sb.Length > 0) sb.Length--;
        return sb;
    }
    
    /// <summary>
    ///     Creates a ComponentType array from the <see cref="parameterSymbols"/> passed through.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/>.</param>
    /// <param name="parameterSymbols">The <see cref="IList{T}"/> with <see cref="ITypeSymbol"/>s which we wanna create a ComponentType array for.</param>
    /// <returns></returns>
    public static StringBuilder GetTypeArray(this StringBuilder sb, IList<ITypeSymbol> parameterSymbols)
    {
        if (parameterSymbols.Count == 0)
        {
            sb.Append("Array.Empty<ComponentType>()");
            return sb;
        }

        sb.Append("new ComponentType[]{");
        
        foreach (var symbol in parameterSymbols)
            if(symbol.Name is not "Entity") // Prevent entity being added to the type array
                sb.Append($"typeof({symbol}),");
        
        if (sb.Length > 0) sb.Length -= 1;
        sb.Append('}');

        return sb;
    }


    /// <summary>
    ///     Appends a set of <see cref="parameterSymbols"/> if they are marked by the data attribute.
    ///     <example>ref gameTime, out somePassedList,...</example>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> of <see cref="IParameterSymbol"/>s which will be appended if they are marked with data.</param>
    /// <returns></returns>
    public static StringBuilder DataParameters(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        sb.Append(',');
        foreach (var parameter in parameterSymbols)
        {
            if (parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data")))
                sb.Append($"{CommonUtils.RefKindToString(parameter.RefKind)} {parameter.Type} {parameter.Name.ToLower()},");
        }
        sb.Length--;
        return sb;
    }
    
    /// <summary>
    ///     Appends method calls made with their important data parameters.
    ///     <example>someQuery(World, gameTime); ...</example>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="methodNames">The <see cref="IEnumerable{T}"/> of methods which we wanna call.</param>
    /// <returns></returns>
    public static StringBuilder CallMethods(this StringBuilder sb, IEnumerable<IMethodSymbol> methodNames)
    {
        foreach (var method in methodNames)
        {
            var data = new StringBuilder();
            data.Append(',');
            foreach (var parameter in method.Parameters)
            {
                if (!parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data"))) continue;
                data.Append($"{CommonUtils.RefKindToString(parameter.RefKind)} Data,");
                break;
            }
            data.Length--;
            sb.AppendLine($"{method.Name}Query(World {data});");   
        }
        return sb;
    }

    /// <summary>
    ///     Gets all the types of a <see cref="AttributeData"/> as <see cref="ITypeSymbol"/>s and adds them to a list.
    ///     If the attribute is generic it will add the generic parameters, if its non generic it will add the non generic types from the constructor.
    /// </summary>
    /// <param name="data">The <see cref="AttributeData"/>.</param>
    /// <param name="array">The <see cref="List{T}"/> where the found <see cref="ITypeSymbol"/>s are added to.</param>
    public static void GetAttributeTypes(AttributeData data, List<ITypeSymbol> array)
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
    
    /// <summary>
    ///     Adds a query without an entity for a given annotated method. The attributes of these methods are used to generate the query.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> which is annotated for source generation.</param>
    /// <returns></returns>
    public static StringBuilder AppendQueryWithoutEntity(this StringBuilder sb, IMethodSymbol methodSymbol)
    {

        var staticModifier = methodSymbol.IsStatic ? "static" : "";
        
        // Get attributes
        var allAttributeData = methodSymbol.GetAttributeData("All");
        var anyAttributeData = methodSymbol.GetAttributeData("Any");
        var noneAttributeData = methodSymbol.GetAttributeData("None");
        var exclusiveAttributeData = methodSymbol.GetAttributeData("Exclusive");
        
        // Get params / components except those marked with data or entities. 
        var components = methodSymbol.Parameters.ToList();
        components.RemoveAll(symbol => symbol.Type.Name.Equals("Entity"));                                                // Remove entitys 
        components.RemoveAll(symbol => symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data")));    // Remove data annotated params
        
        // Create all query array
        var allArray = components.Select(symbol => symbol.Type).ToList();
        var anyArray = new List<ITypeSymbol>();
        var noneArray = new List<ITypeSymbol>();
        var exclusiveArray = new List<ITypeSymbol>();

        // Get All<...> or All(...) passed types and pass them to the arrays 
        GetAttributeTypes(allAttributeData, allArray);
        GetAttributeTypes(anyAttributeData, anyArray);
        GetAttributeTypes(noneAttributeData, noneArray);
        GetAttributeTypes(exclusiveAttributeData, exclusiveArray);
        
        // Remove doubles and entities 
        allArray = allArray.Distinct().ToList();
        anyArray = anyArray.Distinct().ToList();
        noneArray = noneArray.Distinct().ToList();
        exclusiveArray = exclusiveArray.Distinct().ToList();
        
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity")); 
        anyArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));
        noneArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));
        exclusiveArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));

        // Generate code
        var data = new StringBuilder().DataParameters(methodSymbol.Parameters);
        var getArrays = new StringBuilder().GetArrays(components);
        var getFirstElements = new StringBuilder().GetFirstElements(components);
        var getComponents = new StringBuilder().GetComponents(components);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyArray);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneArray);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveArray);

        var template = 
            $$"""
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Utils.Component;
            {{(!methodSymbol.ContainingNamespace.IsGlobalNamespace ? $"namespace {methodSymbol.ContainingNamespace} {{" : "")}}
                public {{staticModifier}} partial class {{methodSymbol.ContainingSymbol.Name}}{
                    
                    private {{staticModifier}} QueryDescription {{methodSymbol.Name}}_QueryDescription = new QueryDescription{
                        All = {{allTypeArray}},
                        Any = {{anyTypeArray}},
                        None = {{noneTypeArray}},
                        Exclusive = {{exclusiveTypeArray}}
                    };

                    private {{staticModifier}} bool _{{methodSymbol.Name}}_Initialized;
                    private {{staticModifier}} Query _{{methodSymbol.Name}}_Query;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public {{staticModifier}} void {{methodSymbol.Name}}Query(World world {{data}}){
                     
                        if(!_{{methodSymbol.Name}}_Initialized){
                            _{{methodSymbol.Name}}_Query = world.Query(in {{methodSymbol.Name}}_QueryDescription);
                            _{{methodSymbol.Name}}_Initialized = true;
                        }

                        foreach(ref var chunk in _{{methodSymbol.Name}}_Query.GetChunkIterator()){
                            
                            var chunkSize = chunk.Size;
                            {{getArrays}}
                            {{getFirstElements}}

                            for (var entityIndex = chunkSize - 1; entityIndex >= 0; --entityIndex)
                            {
                                {{getComponents}}
                                {{methodSymbol.Name}}({{insertParams}});
                            }
                        }
                    }
                }
            {{(!methodSymbol.ContainingNamespace.IsGlobalNamespace ? "}" : "")}}
            """;
        sb.Append(template);
        return sb;
    }
    
    /// <summary>
    ///     Adds a query with an entity for a given annotated method. The attributes of these methods are used to generate the query.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> which is annotated for source generation.</param>
    /// <returns></returns>
    public static StringBuilder AppendQueryWithEntity(this StringBuilder sb, IMethodSymbol methodSymbol)
    {
       var staticModifier = methodSymbol.IsStatic ? "static" : "";
        
        // Get attributes
        var attributeData = methodSymbol.GetAttributeData("All");
        var anyAttributeData = methodSymbol.GetAttributeData("Any");
        var noneAttributeData = methodSymbol.GetAttributeData("None");
        var exclusiveAttributeData = methodSymbol.GetAttributeData("Exclusive");
        
        // Get params / components except those marked with data or entities. 
        var components = methodSymbol.Parameters.ToList();
        components.RemoveAll(symbol => symbol.Type.Name.Equals("Entity"));                                                // Remove entitys 
        components.RemoveAll(symbol => symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data")));    // Remove data annotated params
        
        // Create all query array
        var allArray = components.Select(symbol => symbol.Type).ToList();
        var anyArray = new List<ITypeSymbol>();
        var noneArray = new List<ITypeSymbol>();
        var exclusiveArray = new List<ITypeSymbol>();

        // Get All<...> or All(...) passed types and pass them to the arrays 
        GetAttributeTypes(attributeData, allArray);
        GetAttributeTypes(anyAttributeData, anyArray);
        GetAttributeTypes(noneAttributeData, noneArray);
        GetAttributeTypes(exclusiveAttributeData, exclusiveArray);
        
        // Remove doubles and entities 
        allArray = allArray.Distinct().ToList();
        anyArray = anyArray.Distinct().ToList();
        noneArray = noneArray.Distinct().ToList();
        exclusiveArray = exclusiveArray.Distinct().ToList();
        
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity")); 
        anyArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));
        noneArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));
        exclusiveArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));
        
        // Generate code 
        var data = new StringBuilder().DataParameters(methodSymbol.Parameters);
        var getArrays = new StringBuilder().GetArrays(components);
        var getFirstElements = new StringBuilder().GetFirstElements(components);
        var getComponents = new StringBuilder().GetComponents(components);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyArray);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneArray);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveArray);

        var template = 
            $$"""
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Utils.Component;
            {{(!methodSymbol.ContainingNamespace.IsGlobalNamespace ? $"namespace {methodSymbol.ContainingNamespace} {{" : "")}}
                public {{staticModifier}} partial class {{methodSymbol.ContainingSymbol.Name}}{
                    
                    private {{staticModifier}} QueryDescription {{methodSymbol.Name}}_QueryDescription = new QueryDescription{
                        All = {{allTypeArray}},
                        Any = {{anyTypeArray}},
                        None = {{noneTypeArray}},
                        Exclusive = {{exclusiveTypeArray}}
                    };

                    private {{staticModifier}} bool _{{methodSymbol.Name}}_Initialized;
                    private {{staticModifier}} Query _{{methodSymbol.Name}}_Query;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public {{staticModifier}} void {{methodSymbol.Name}}Query(World world {{data}}){
                     
                        if(!_{{methodSymbol.Name}}_Initialized){
                            _{{methodSymbol.Name}}_Query = world.Query(in {{methodSymbol.Name}}_QueryDescription);
                            _{{methodSymbol.Name}}_Initialized = true;
                        }

                        foreach(ref var chunk in _{{methodSymbol.Name}}_Query.GetChunkIterator()){
                            
                            var chunkSize = chunk.Size;
                            {{getArrays}}
                            ref var entityFirstElement = ref ArrayExtensions.DangerousGetReference(chunk.Entities);
                            {{getFirstElements}}

                            for (var entityIndex = chunkSize - 1; entityIndex >= 0; --entityIndex)
                            {
                                ref readonly var entity = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                                {{getComponents}}
                                {{methodSymbol.Name}}({{insertParams}});
                            }
                        }
                    }
                }
            {{(!methodSymbol.ContainingNamespace.IsGlobalNamespace ? "}" : "")}}
            """;

        sb.Append(template);
        return sb;
    }
}