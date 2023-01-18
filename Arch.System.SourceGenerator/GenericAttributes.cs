using System.Text;

namespace Arch.System.SourceGenerator;

public static class GenericAttributesUtils
{
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
}