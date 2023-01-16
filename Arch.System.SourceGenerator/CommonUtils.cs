using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class CommonUtils
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
}