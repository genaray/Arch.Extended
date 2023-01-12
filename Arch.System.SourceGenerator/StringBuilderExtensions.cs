using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class StringBuilderExtensions
{
    
    public static string RefKindToString(RefKind refKind)
    {
        switch (refKind)
        {
            case RefKind.None:
                return "";
            case RefKind.Ref:
                return "ref";
            case RefKind.In:
                return "in";
            case RefKind.Out:
                return "out";
        }
        return null;
    }
    
    public static StringBuilder GenericsWithoutBrackets(this StringBuilder sb, int index)
    {
        for (var i = 0; i < index; i++)
            sb.Append($"T{i},");
        if (sb.Length > 0) sb.Length -= 1;

        return sb;
    }
    
    public static StringBuilder GenericsWithoutBrackets(this StringBuilder sb, ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        foreach (var method in parameterSymbols)
            sb.Append(method.Type.Name+",");
        if (sb.Length > 0) sb.Length -= 1;

        return sb;
    }
    
    public static StringBuilder GenericsToTypeArray(this StringBuilder sb, int index)
    {
        for (var i = 0; i < index; i++)
            sb.Append($"typeof(T{i}),");
        if (sb.Length > 0) sb.Length -= 1;

        return sb;
    }
    
    public static StringBuilder GetArrays(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity" || !symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data"))) // Prevent entity being added to the type array
                sb.AppendLine($"var {symbol.Type.Name.ToLower()}Array = chunk.GetArray<{symbol.Type}>();");

        return sb;
    }
    
    public static StringBuilder GetFirstElements(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
      
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Type.Name.ToLower()}FirstElement = ref ArrayExtensions.DangerousGetReference({symbol.Type.Name.ToLower()}Array);");

        return sb;
    }
    
    public static StringBuilder GetComponents(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Name.ToLower()} = ref Unsafe.Add(ref {symbol.Type.Name.ToLower()}FirstElement, entityIndex);");

        return sb;
    }
    
    public static StringBuilder InsertParams(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            sb.Append($"{RefKindToString(symbol.RefKind)} {symbol.Name.ToLower()},");
        
        if(sb.Length > 0) sb.Length--;
        return sb;
    }
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
    
    public static StringBuilder GetTypeArray(this StringBuilder sb, INamedTypeSymbol attributeSymbol)
    {
        if (attributeSymbol is not null)
            sb.GetTypeArray(attributeSymbol.TypeArguments);
        else sb.AppendLine("Array.Empty<ComponentType>()");
        
        return sb;
    }

    public static StringBuilder DataParameters(this StringBuilder sb, IEnumerable<IParameterSymbol> parameterSymbols)
    {
        sb.Append(',');
        foreach (var parameter in parameterSymbols)
        {
            if (parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data")))
                sb.Append($"{RefKindToString(parameter.RefKind)} {parameter.Type} {parameter.Name.ToLower()},");
        }
        sb.Length--;
        return sb;
    }
    
    public static StringBuilder CallMethods(this StringBuilder sb, IEnumerable<IMethodSymbol> methodNames)
    {
        foreach (var method in methodNames)
        {
            var data = new StringBuilder();
            data.Append(',');
            foreach (var parameter in method.Parameters)
            {
                if (!parameter.GetAttributes().Any(attributeData => attributeData.AttributeClass.Name.Contains("Data"))) continue;
                data.Append($"{RefKindToString(parameter.RefKind)} Data,");
                break;
            }
            data.Length--;
            sb.AppendLine($"{method.Name}Query(World {data});");   
        }
        return sb;
    }
    
    public static StringBuilder AppendGenericAttributes(this StringBuilder sb, string name, string parent, int index)
    {
        for (var i = 1; i < index; i++)
            sb.AppendGenericAttribute(name, parent, i);
        return sb;
    }

    public static StringBuilder AppendGenericAttribute(this StringBuilder sb, string name, string parent, int index)
    {
        var generics = new StringBuilder().GenericsWithoutBrackets(index);
        var genericsToTypeArray = new StringBuilder().GenericsToTypeArray(index);
        
        var template = $$"""
        public class {{name}}Attribute<{{generics}}> : {{parent}}Attribute
        {
            public {{name}}Attribute(): base({{genericsToTypeArray}}){}
        }
        """;

        sb.AppendLine(template);
        return sb;
    }

    public static StringBuilder AppendQueryWithoutEntity(this StringBuilder sb, IMethodSymbol methodSymbol)
    {

        var staticModifier = methodSymbol.IsStatic ? "static" : "";
        
        // Get attributes
        var allAttributeSymbol = methodSymbol.GetAttribute("All");
        var anyAttributeSymbol = methodSymbol.GetAttribute("Any");
        var noneAttributeSymbol = methodSymbol.GetAttribute("None");
        var exclusiveAttributeSymbol = methodSymbol.GetAttribute("Exclusive");
        
        // Get params / components except those marked with data or entities. 
        var components = methodSymbol.Parameters.ToList();
        components.RemoveAll(symbol => symbol.Type.Name.Equals("Entity"));                                                // Remove entitys 
        components.RemoveAll(symbol => symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data")));    // Remove data annotated params
        
        // Create all query array
        var allArray = components.Select(symbol => symbol.Type).ToList();
        if(allAttributeSymbol is not null) allArray.AddRange(allAttributeSymbol.TypeArguments);
        allArray = allArray.Distinct().ToList();
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));  // Do not allow entities inside All

        // Generate code
        var data = new StringBuilder().DataParameters(methodSymbol.Parameters);
        var getArrays = new StringBuilder().GetArrays(components);
        var getFirstElements = new StringBuilder().GetFirstElements(components);
        var getComponents = new StringBuilder().GetComponents(components);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyAttributeSymbol);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneAttributeSymbol);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveAttributeSymbol);

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
            namespace {{methodSymbol.ContainingNamespace}}{
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
            }
            """;
        sb.Append(template);
        return sb;
    }
    
    public static StringBuilder AppendQueryWithEntity(this StringBuilder sb, IMethodSymbol methodSymbol)
    {
       var staticModifier = methodSymbol.IsStatic ? "static" : "";
        
        // Get attributes
        var allAttributeSymbol = methodSymbol.GetAttribute("All");
        var anyAttributeSymbol = methodSymbol.GetAttribute("Any");
        var noneAttributeSymbol = methodSymbol.GetAttribute("None");
        var exclusiveAttributeSymbol = methodSymbol.GetAttribute("Exclusive");
        
        // Get params / components except those marked with data or entities. 
        var components = methodSymbol.Parameters.ToList();
        components.RemoveAll(symbol => symbol.Type.Name.Equals("Entity"));                                                // Remove entitys 
        components.RemoveAll(symbol => symbol.GetAttributes().Any(data => data.AttributeClass.Name.Contains("Data")));    // Remove data annotated params
        
        // Create all query array
        var allArray = components.Select(symbol => symbol.Type).ToList();
        if(allAttributeSymbol is not null) allArray.AddRange(allAttributeSymbol.TypeArguments);
        allArray = allArray.Distinct().ToList();
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));  // Do not allow entities inside All
        
        // Generate code 
        var data = new StringBuilder().DataParameters(methodSymbol.Parameters);
        var getArrays = new StringBuilder().GetArrays(components);
        var getFirstElements = new StringBuilder().GetFirstElements(components);
        var getComponents = new StringBuilder().GetComponents(components);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);
        
        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyAttributeSymbol);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneAttributeSymbol);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveAttributeSymbol);

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
            namespace {{methodSymbol.ContainingNamespace}}{
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
            }
            """;

        sb.Append(template);
        return sb;
    }
}