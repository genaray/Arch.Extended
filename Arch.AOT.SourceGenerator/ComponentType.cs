namespace Arch.AOT.SourceGenerator;

/// <summary>
///     The struct <see cref="ComponentType"/>
///		represents an Component (Their type with meta data) for use in the generated code.
/// </summary>
public struct ComponentType
{
	/// <summary>
	///     The type name of the component.
	/// </summary>
	public string TypeName { get; }
	/// <summary>
	///     If the component has zero fields.
	/// </summary>
	public bool IsZeroSize { get; }
	/// <summary>
	///     If the component is a value type.
	/// </summary>
	public bool IsValueType { get; }

	/// <summary>
	///		Creates a new instance of the <see cref="ComponentType"/>.
	/// </summary>
	/// <param name="typeName">The type name.</param>
	/// <param name="isZeroSize">If its zero sized.</param>
	/// <param name="isValueType">If its a value type.</param>
	public ComponentType(string typeName, bool isZeroSize, bool isValueType)
	{
		TypeName = typeName;
		IsZeroSize = isZeroSize;
		IsValueType = isValueType;
	}
}