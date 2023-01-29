using System.Text;

namespace Arch.System.SourceGenerator;

public static class GenericAttributesUtils
{
    
    /// <summary>
    ///     Appends some generic attributes.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="parent">Its parent.</param>
    /// <param name="amount">The amount.</param>
    /// <returns></returns>
    public static StringBuilder AppendGenericAttributes(this StringBuilder sb, string name, string parent, int amount)
    {
        for (var i = 1; i < amount; i++)
            sb.AppendGenericAttribute(name, parent, i);
        return sb;
    }

    /// <summary>
    ///     Appends one generic attribute.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> instance.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="parent">Its parent.</param>
    /// <param name="amount">The amount.</param>
    /// <returns></returns>
    public static StringBuilder AppendGenericAttribute(this StringBuilder sb, string name, string parent, int amount)
    {
        var generics = new StringBuilder().GenericsWithoutBrackets(amount);
        var genericsToTypeArray = new StringBuilder().GenericsToTypeArray(amount);
        
        var template = $$"""
        public class {{name}}Attribute<{{generics}}> : {{parent}}Attribute
        {
            public {{name}}Attribute(): base({{genericsToTypeArray}}){}
        }
        """;

        sb.AppendLine(template);
        return sb;
    }
}