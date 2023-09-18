using System.Text;

namespace Arch.SourceGen.Extensions;

public static class StringBuilderExtensions
{
	/// <summary>
	///     Appends the component types to the string builder in the form of a generated class.
	/// </summary>
	/// <param name="sb">The target string builder.</param>
	/// <param name="componentTypes">The types to append.</param>
	/// <returns></returns>
	public static StringBuilder AppendComponentTypes(this StringBuilder sb, IList<ComponentType> componentTypes)
	{
		sb.AppendLine(@"using System.Runtime.CompilerServices;
using Arch.Core.Utils;

namespace Arch.Generated
{
    internal static class GeneratedComponentRegistry
    {
        [ModuleInitializer]
        public static void Initialize()
        {");

		for (int i = 0; i < componentTypes.Count; i++)
		{
			ComponentType componentType = componentTypes[i];
			sb.AppendComponentType(ref componentType);
		}

		sb.Append(@"
		}
	}
}");

		return sb;
	}

	/// <summary>
	///     Appends a single component type to the string builder as a new line.
	/// </summary>
	/// <param name="sb">The string builder.</param>
	/// <param name="componentType">The component type to add.</param>
	/// <returns></returns>
	public static StringBuilder AppendComponentType(this StringBuilder sb, ref ComponentType componentType)
	{
		string hasZeroFieldsString = componentType.IsZeroSize ? "true" : "false";
		string size = componentType.IsValueType ? $"Unsafe.SizeOf<{componentType.TypeName}>()" : "IntPtr.Size";

		sb.AppendLine(
			$"\t\t\tComponentRegistry.Add(new ComponentType(ComponentRegistry.Size + 1, typeof({componentType.TypeName}), {size}, {hasZeroFieldsString}));");

		return sb;
	}
}