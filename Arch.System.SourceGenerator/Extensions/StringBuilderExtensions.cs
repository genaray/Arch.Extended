using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.System.SourceGenerator;

public static class CommonUtils
{
    
    /// <summary>
    ///     Convert a <see cref="RefKind"/> to its code string equivalent.
    /// </summary>
    /// <param name="refKind">The <see cref="RefKind"/>.</param>
    /// <returns>The code string equivalent.</returns>
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
    
    /// <summary>
    ///     Creates a list of generic type parameters separated by a simple comma.
    ///     <example>T0,T1,..TN</example> 
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="amount">The amount of generic type parameters.</param>
    /// <returns></returns>
    public static StringBuilder GenericsWithoutBrackets(this StringBuilder sb, int amount)
    {
        for (var i = 0; i < amount; i++)
            sb.Append($"T{i},");
        if (sb.Length > 0) sb.Length -= 1;

        return sb;
    }
    
    /// <summary>
    ///     Creates a list of generic type parameters types separated by a simple comma.
    ///     <example>typeof(T0),typeof(T1),..typeof(TN)</example> 
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="amount">The amount of generic type parameters.</param>
    /// <returns></returns>
    public static StringBuilder GenericsToTypeArray(this StringBuilder sb, int amount)
    {
        for (var i = 0; i < amount; i++)
            sb.Append($"typeof(T{i}),");
        if (sb.Length > 0) sb.Length -= 1;

        return sb;
    }
}