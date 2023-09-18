namespace Arch.SourceGen;

/// <summary>
///     Represents ComponentType for use in the generated code.
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

	public ComponentType(string typeName, bool isZeroSize, bool isValueType)
	{
		TypeName = typeName;
		IsZeroSize = isZeroSize;
		IsValueType = isValueType;
	}
}