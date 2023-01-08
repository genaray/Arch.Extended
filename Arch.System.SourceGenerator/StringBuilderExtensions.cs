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
    
    public static StringBuilder GetArrays(this StringBuilder sb, ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"var {symbol.Type.Name}Array = chunk.GetArray<{symbol.Type.Name}>();");

        return sb;
    }
    
    public static StringBuilder GetFirstElements(this StringBuilder sb, ImmutableArray<IParameterSymbol> parameterSymbols)
    {
      
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Type.Name}FirstElement = ref ArrayExtensions.DangerousGetReference({symbol.Type.Name}Array);");

        return sb;
    }
    
    public static StringBuilder GetComponents(this StringBuilder sb, ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            if(symbol.Type.Name is not "Entity") // Prevent entity being added to the type array
                sb.AppendLine($"ref var {symbol.Type.Name}Component = ref Unsafe.Add(ref {symbol.Type.Name}FirstElement, entityIndex);");

        return sb;
    }
    
    public static StringBuilder InsertParams(this StringBuilder sb, ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        foreach (var symbol in parameterSymbols)
            sb.Append($"{RefKindToString(symbol.RefKind)} {symbol.Type.Name}Component,");
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
                sb.Append($"typeof({symbol.Name}),");
        
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
    
    public static StringBuilder GetMethods(this StringBuilder sb, List<string> methodNames)
    {
        foreach (var method in methodNames)
            sb.AppendLine($"{method}();");
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
        var getArrays = new StringBuilder().GetArrays(methodSymbol.Parameters);
        var getFirstElements = new StringBuilder().GetFirstElements(methodSymbol.Parameters);
        var getComponents = new StringBuilder().GetComponents(methodSymbol.Parameters);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);

        var allAttributeSymbol = methodSymbol.GetAttribute("All");
        var anyAttributeSymbol = methodSymbol.GetAttribute("Any");
        var noneAttributeSymbol = methodSymbol.GetAttribute("None");
        var exclusiveAttributeSymbol = methodSymbol.GetAttribute("Exclusive");
        
        var allArray = methodSymbol.Parameters.Select(symbol => symbol.Type).ToList();
        if(allAttributeSymbol is not null) allArray.AddRange(allAttributeSymbol.TypeArguments);
        allArray = allArray.DistinctBy(symbol => symbol.Name).ToList();
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));

        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyAttributeSymbol);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneAttributeSymbol);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveAttributeSymbol);

        var template = 
            $$"""
            using System;
            using System.Collections.Generic;
            using System.Collections.Specialized;
            using System.ComponentModel;
            using System.Linq;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using Collections.Pooled;
            using JobScheduler;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Utils.Component;
            using System.Runtime.CompilerServices;
            namespace {{methodSymbol.ContainingNamespace}};
            public partial class {{methodSymbol.ContainingSymbol.Name}}{
                
                private QueryDescription {{methodSymbol.Name}}_QueryDescription = new QueryDescription{
                    All = {{allTypeArray}},
                    Any = {{anyTypeArray}},
                    None = {{noneTypeArray}},
                    Exclusive = {{exclusiveTypeArray}}
                };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void {{methodSymbol.Name}}Query(){
                 
                    var query = World.Query(in {{methodSymbol.Name}}_QueryDescription);
                    foreach(ref var chunk in query.GetChunkIterator()){
                        
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
            """;
        sb.Append(template);
        return sb;
    }
    
    public static StringBuilder AppendQueryWithEntity(this StringBuilder sb, IMethodSymbol methodSymbol)
    {
        var getArrays = new StringBuilder().GetArrays(methodSymbol.Parameters);
        var getFirstElements = new StringBuilder().GetFirstElements(methodSymbol.Parameters);
        var getComponents = new StringBuilder().GetComponents(methodSymbol.Parameters);
        var insertParams = new StringBuilder().InsertParams(methodSymbol.Parameters);

        var allAttributeSymbol = methodSymbol.GetAttribute("All");
        var anyAttributeSymbol = methodSymbol.GetAttribute("Any");
        var noneAttributeSymbol = methodSymbol.GetAttribute("None");
        var exclusiveAttributeSymbol = methodSymbol.GetAttribute("Exclusive");
        
        var allArray = methodSymbol.Parameters.Select(symbol => symbol.Type).ToList();
        if(allAttributeSymbol is not null) allArray.AddRange(allAttributeSymbol.TypeArguments);
        allArray = allArray.DistinctBy(symbol => symbol.Name).ToList();
        allArray.RemoveAll(symbol => symbol.Name.Equals("Entity"));

        var allTypeArray = new StringBuilder().GetTypeArray(allArray);
        var anyTypeArray = new StringBuilder().GetTypeArray(anyAttributeSymbol);
        var noneTypeArray = new StringBuilder().GetTypeArray(noneAttributeSymbol);
        var exclusiveTypeArray = new StringBuilder().GetTypeArray(exclusiveAttributeSymbol);
        
        var template = 
            $$"""
            using System;
            using System.Collections.Generic;
            using System.Collections.Specialized;
            using System.ComponentModel;
            using System.Linq;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            using Arch.Core;
            using Arch.Core.Extensions;
            using Arch.Core.Utils;
            using Collections.Pooled;
            using JobScheduler;
            using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
            using Component = Arch.Core.Utils.Component;
            using System.Runtime.CompilerServices;
            namespace {{methodSymbol.ContainingNamespace}};
            public partial class {{methodSymbol.ContainingSymbol.Name}}{
                
                private QueryDescription {{methodSymbol.Name}}_QueryDescription = new QueryDescription{            
                    All = {{allTypeArray}},
                    Any = {{anyTypeArray}},
                    None = {{noneTypeArray}},
                    Exclusive = {{exclusiveTypeArray}}    
                };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void {{methodSymbol.Name}}Query(){
                 
                    var query = World.Query(in {{methodSymbol.Name}}_QueryDescription);
                    foreach(ref var chunk in query.GetChunkIterator()){
                        
                        var chunkSize = chunk.Size;
                        {{getArrays}}
                        ref var entityFirstElement = ref ArrayExtensions.DangerousGetReference(chunk.Entities);
                        {{getFirstElements}}

                        for (var entityIndex = chunkSize - 1; entityIndex >= 0; --entityIndex)
                        {
                            ref readonly var EntityComponent = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                            {{getComponents}}
                            {{methodSymbol.Name}}({{insertParams}});
                        }
                    }
                }
            }
            """;
        
        sb.Append(template);
        return sb;
    }
}