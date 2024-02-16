using System.Text;

namespace Arch.AOT.SourceGenerator.Extensions;

/// <summary>
///		The <see cref="StringBuilderExtensions"/> class
///		adds code-generating methods to the string-builder for outsourcing them.
/// </summary>
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
		// Lists the component registration commands line by line. 
		var components = new StringBuilder();
		foreach (var type in componentTypes)
		{
			var componentType = type;
			components.AppendComponentType(ref componentType);
		}
		
		sb.AppendLine(
			$$"""
		    using System.Runtime.CompilerServices;
		    using Arch.Core.Utils;
		              
		    namespace Arch.AOT.SourceGenerator
		    {
		       internal static class GeneratedComponentRegistry
		       {
		          [ModuleInitializer]
		          public static void Initialize()
		          {
		          {{components}}
		          }
		       }
		    }
		    """
		);
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
		//var size = componentType.IsValueType ? $"Unsafe.SizeOf<{componentType.TypeName}>()" : "IntPtr.Size";
		//sb.AppendLine($"ComponentRegistry.Add(typeof({componentType.TypeName}), new ComponentType(ComponentRegistry.Size + 1, {size}));");
		
		sb.AppendLine($"ArrayRegistry.Add<{componentType.TypeName}>();");
		return sb;
	}
}
