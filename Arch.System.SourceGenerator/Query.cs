using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;

namespace Arch.System.SourceGenerator;

public static class QueryUtils
{

    /// <summary>
    ///     Appends the first elements of the types specified in the <see cref="parameterSymbols"/> from the previous specified arrays.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> list of <see cref="IParameterSymbol"/>s which we wanna append the first elements for.</param>
    /// <returns></returns>
    public static StringBuilder GetFirstElements(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
      
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity" || !symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data"))) // Prevent entity being added to the type array
                sb.AppendLine($"ref var @{symbol.Type.Name.ToLower()}FirstElement = ref chunk.GetFirst<{symbol.Type.ToDisplayString(NullableFlowState.None, SymbolDisplayFormat.FullyQualifiedFormat)}>();");

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
                sb.AppendLine($"ref var @{symbol.Name.ToLower()} = ref Unsafe.Add(ref {symbol.Type.Name.ToLower()}FirstElement, entityIndex);");

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
            sb.Append($"{CommonUtils.RefKindToString(symbol.RefKind)} @{symbol.Name.ToLower()},");
        
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
            sb.Append("Signature.Null");
            return sb;
        }

        sb.Append("new Signature(");
        
        foreach (var symbol in parameterSymbols)
            if(symbol.Name is not "Entity") // Prevent entity being added to the type array
                sb.Append($"typeof({symbol.ToDisplayString(NullableFlowState.None, SymbolDisplayFormat.FullyQualifiedFormat)}),");
        
        if (sb.Length > 0) sb.Length -= 1;
        sb.Append(')');

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
                sb.Append($"{CommonUtils.RefKindToString(parameter.RefKind)} {parameter.Type} @{parameter.Name.ToLower()},");
        }
        sb.Length--;
        return sb;
    }
    
    /// <summary>
    ///     Appends a set of <see cref="parameterSymbols"/> if they are marked by the data attribute.
    ///     <example>ref gameTime, out somePassedList,...</example>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> of <see cref="IParameterSymbol"/>s which will be appended if they are marked with data.</param>
    /// <returns></returns>
    public static StringBuilder JobParameters(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var parameter in parameterSymbols)
        {
            if (parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data")))
                sb.AppendLine($"public {parameter.Type} @{parameter.Name.ToLower()};");
        }
        return sb;
    }
    
    /// <summary>
    ///     Appends a set of <see cref="parameterSymbols"/> if they are marked by the data attribute.
    ///     <example>ref gameTime, out somePassedList,...</example>
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="parameterSymbols">The <see cref="IEnumerable{T}"/> of <see cref="IParameterSymbol"/>s which will be appended if they are marked with data.</param>
    /// <returns></returns>
    public static StringBuilder JobParametersAssigment(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        bool found = false;
        foreach (var parameter in parameterSymbols)
        {
            if (parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data")))
            {
                found = true;
                sb.Append($"@{parameter.Name.ToLower()} = @{parameter.Name.ToLower()},");
            }
        }
        if (found) sb.Length--;
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
                data.Append($"{CommonUtils.RefKindToString(parameter.RefKind)} data,");
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
    ///     Adds a query with an entity for a given annotated method. The attributes of these methods are used to generate the query.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> which is annotated for source generation.</param>
    /// <returns></returns>
    public static StringBuilder AppendQueryMethod(this StringBuilder sb, IMethodSymbol methodSymbol)
    {

        // Check for entity param
        var entity = methodSymbol.Parameters.Any(symbol => symbol.Type.Name.Equals("Entity"));
        var entityParam = entity ? methodSymbol.Parameters.First(symbol => symbol.Type.Name.Equals("Entity")) : null;

        var queryData = methodSymbol.GetAttributeData("Query");
        bool isParallel = (bool)(queryData.NamedArguments.FirstOrDefault(d => d.Key == "Parallel").Value.Value ?? false);

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

        // Create data modell and generate it
        var className = methodSymbol.ContainingSymbol.ToString();
        var queryMethod = new QueryMethod
        {
            IsGlobalNamespace = methodSymbol.ContainingNamespace.IsGlobalNamespace,
            Namespace = methodSymbol.ContainingNamespace.ToString(),
            ClassName = className.Substring(className.LastIndexOf('.')+1),
            
            IsStatic = methodSymbol.IsStatic,
            IsEntityQuery = entity,
            MethodName = methodSymbol.Name,
            
            EntityParameter = entityParam,
            Parameters = methodSymbol.Parameters,
            Components = components,
            
            AllFilteredTypes = allArray,
            AnyFilteredTypes = anyArray,
            NoneFilteredTypes = noneArray,
            ExclusiveFilteredTypes = exclusiveArray
        };
        
        return isParallel ? sb.AppendParallelQueryMethod(ref queryMethod) : sb.AppendQueryMethod(ref queryMethod);
    }

    /// <summary>
    ///     Adds a query with an entity for a given annotated method. The attributes of these methods are used to generate the query.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="queryMethod">The <see cref="QueryMethod"/> which is generated.</param>
    /// <returns></returns>
    public static StringBuilder AppendQueryMethod(this StringBuilder sb, ref QueryMethod queryMethod)
    {
        var staticModifier = queryMethod.IsStatic ? "static" : "";
        
        // Generate code 
        var data = new StringBuilder().DataParameters(queryMethod.Parameters);
        var getFirstElements = new StringBuilder().GetFirstElements(queryMethod.Components);
        var getComponents = new StringBuilder().GetComponents(queryMethod.Components);
        var insertParams = new StringBuilder().InsertParams(queryMethod.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(queryMethod.AllFilteredTypes);
        var anyTypeArray = new StringBuilder().GetTypeArray(queryMethod.AnyFilteredTypes);
        var noneTypeArray = new StringBuilder().GetTypeArray(queryMethod.NoneFilteredTypes);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(queryMethod.ExclusiveFilteredTypes);

        var template = 
            $$"""
            #nullable enable
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Component;
            {{(!queryMethod.IsGlobalNamespace ? $"namespace {queryMethod.Namespace} {{" : "")}}
                partial class {{queryMethod.ClassName}}{
                    
                    private {{staticModifier}} QueryDescription {{queryMethod.MethodName}}_QueryDescription = new QueryDescription(
                        all: {{allTypeArray}},
                        any: {{anyTypeArray}},
                        none: {{noneTypeArray}},
                        exclusive: {{exclusiveTypeArray}}
                    );

                    private {{staticModifier}} World? _{{queryMethod.MethodName}}_Initialized;
                    private {{staticModifier}} Query? _{{queryMethod.MethodName}}_Query;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public {{staticModifier}} void {{queryMethod.MethodName}}Query(World world {{data}}){
                     
                        if(!ReferenceEquals(_{{queryMethod.MethodName}}_Initialized, world)) {
                            _{{queryMethod.MethodName}}_Query = world.Query(in {{queryMethod.MethodName}}_QueryDescription);
                            _{{queryMethod.MethodName}}_Initialized = world;
                        }

                        foreach(ref var chunk in _{{queryMethod.MethodName}}_Query){
                            
                            {{(queryMethod.IsEntityQuery ? "ref var entityFirstElement = ref chunk.Entity(0);" : "")}}
                            {{getFirstElements}}

                            foreach(var entityIndex in chunk)
                            {
                                {{(queryMethod.IsEntityQuery ? $"ref readonly var {queryMethod.EntityParameter.Name.ToLower()} = ref Unsafe.Add(ref entityFirstElement, entityIndex);" : "")}}
                                {{getComponents}}
                                {{queryMethod.MethodName}}({{insertParams}});
                            }
                        }
                    }
                }
            {{(!queryMethod.IsGlobalNamespace ? "}" : "")}}
            """;

        sb.Append(template);
        return sb;
    }

    /// <summary>
    ///     Adds a parallel query with an entity for a given annotated method. The attributes of these methods are used to generate the query.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="queryMethod">The <see cref="QueryMethod"/> which is generated.</param>
    /// <returns></returns>
    public static StringBuilder AppendParallelQueryMethod(this StringBuilder sb, ref QueryMethod queryMethod)
    {
        var staticModifier = queryMethod.IsStatic ? "static" : "";
        
        // Generate code 
        var jobParameters = new StringBuilder().JobParameters(queryMethod.Parameters);
        var jobParametersAssigment = new StringBuilder().JobParametersAssigment(queryMethod.Parameters);
        var data = new StringBuilder().DataParameters(queryMethod.Parameters);
        var getFirstElements = new StringBuilder().GetFirstElements(queryMethod.Components);
        var getComponents = new StringBuilder().GetComponents(queryMethod.Components);
        var insertParams = new StringBuilder().InsertParams(queryMethod.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(queryMethod.AllFilteredTypes);
        var anyTypeArray = new StringBuilder().GetTypeArray(queryMethod.AnyFilteredTypes);
        var noneTypeArray = new StringBuilder().GetTypeArray(queryMethod.NoneFilteredTypes);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(queryMethod.ExclusiveFilteredTypes);

        var template = 
            $$"""
            #nullable enable
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Component;
            {{(!queryMethod.IsGlobalNamespace ? $"namespace {queryMethod.Namespace} {{" : "")}}
                partial class {{queryMethod.ClassName}}{
                    
                    private {{staticModifier}} QueryDescription {{queryMethod.MethodName}}_QueryDescription = new QueryDescription(
                        all: {{allTypeArray}},
                        any: {{anyTypeArray}},
                        none: {{noneTypeArray}},
                        exclusive: {{exclusiveTypeArray}}
                    );

                    private {{staticModifier}} World? _{{queryMethod.MethodName}}_Initialized;
                    private {{staticModifier}} Query? _{{queryMethod.MethodName}}_Query;

                    private struct {{queryMethod.MethodName}}QueryJobChunk : IChunkJob 
                    {
                        {{jobParameters}}
                        
                        public void Execute(ref Chunk chunk) {
                        
                            {{(queryMethod.IsEntityQuery ? "ref var entityFirstElement = ref chunk.Entity(0);" : "")}}
                            {{getFirstElements}}
                    
                            foreach(var entityIndex in chunk)
                            {
                                {{(queryMethod.IsEntityQuery ? $"ref readonly var {queryMethod.EntityParameter.Name.ToLower()} = ref Unsafe.Add(ref entityFirstElement, entityIndex);" : "")}}
                                {{getComponents}}
                                {{queryMethod.MethodName}}({{insertParams}});
                            }
                        }
                    }
            
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public {{staticModifier}} void {{queryMethod.MethodName}}Query(World world {{data}}){
                     
                        if(!ReferenceEquals(_{{queryMethod.MethodName}}_Initialized, world)) {
                            _{{queryMethod.MethodName}}_Query = world.Query(in {{queryMethod.MethodName}}_QueryDescription);
                            _{{queryMethod.MethodName}}_Initialized = world;
                        }
                        
                        var job = new {{queryMethod.MethodName}}QueryJobChunk() { {{jobParametersAssigment}} };
                        world.InlineParallelChunkQuery(in {{queryMethod.MethodName}}_QueryDescription, job);
                    }
                }
            {{(!queryMethod.IsGlobalNamespace ? "}" : "")}}
            """;

        sb.Append(template);
        return sb;
    }

    

    /// <summary>
    ///     Adds a basesystem that calls a bunch of query methods. 
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="classToMethod">The <see cref="KeyValuePair{TKey,TValue}"/> which maps all query methods to a common class containing them.</param>
    /// <returns></returns>
    public static StringBuilder AppendBaseSystem(this StringBuilder sb, KeyValuePair<ISymbol, List<IMethodSymbol>> classToMethod)
    {
        // Get BaseSystem class
        var classSymbol = classToMethod.Key as INamedTypeSymbol;

        INamedTypeSymbol? parentSymbol = null;
        var implementsUpdate = false;
        var type = classSymbol;
        while (type != null)
        {
            // Update was implemented by user, no need to do that by source generator.
            if (type.GetMembers("Update").OfType<IMethodSymbol>().Any(member => member.IsOverride))
                implementsUpdate = true;

            type = type.BaseType;

            // Ignore classes which do not derive from BaseSystem
            if (type?.Name == "BaseSystem")
            {
                parentSymbol = type;
                break;
            }
        }

        if (parentSymbol == null || implementsUpdate)
            return sb;

        // Get generic of BaseSystem
        var typeSymbol = parentSymbol.TypeArguments[1];

        var className = classSymbol.ToString();

        // Generate basesystem.
        var baseSystem = new BaseSystem
        {
            Namespace = classSymbol.ContainingNamespace != null && !classSymbol.ContainingNamespace.IsGlobalNamespace ? classSymbol.ContainingNamespace.ToString() : string.Empty,
            GenericType = typeSymbol,
            GenericTypeNamespace = typeSymbol.ContainingNamespace.ToString(),
            Name = className.Substring(className.LastIndexOf('.') + 1),
            QueryMethods = classToMethod.Value,
        };
        return sb.AppendBaseSystem(ref baseSystem);
    }
    
    /// <summary>
    ///     Adds a basesystem that calls a bunch of query methods. 
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="baseSystem">The <see cref="BaseSystem"/> which is generated.</param>
    /// <returns></returns>
    public static StringBuilder AppendBaseSystem(this StringBuilder sb, ref BaseSystem baseSystem)
    {
        var methodCalls = new StringBuilder().CallMethods(baseSystem.QueryMethods);
        var template =
            $$"""
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using {{baseSystem.GenericTypeNamespace}};
            {{(baseSystem.Namespace != string.Empty ? $"namespace {baseSystem.Namespace} {{" : "")}}
                partial class {{baseSystem.Name}}{
                        
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public override void Update(in {{baseSystem.GenericType.ToDisplayString()}} data){
                        {{methodCalls}}
                    }
                }
            {{(baseSystem.Namespace != string.Empty ? "}" : "")}}
            """;
        return sb.Append(template);
    }
}
