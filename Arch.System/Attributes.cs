namespace Arch.System.SourceGenerator;

/// <summary>
///     Marks an method inside a <see cref="BaseSystem{W,T}"/> for being targeted by the source generator.
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method)]
public class UpdateAttribute : global::System.Attribute
{
}

/// <summary>
///     Defines a set of components each entity requires. 
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public class AllAttribute : global::System.Attribute
{
    /// <summary>
    /// The types of the component.
    /// </summary>
    public Type[] ComponentTypes { get; }

    public AllAttribute(params Type[] componentTypes)
    {
        ComponentTypes = componentTypes;
    }
}

/// <summary>
///     Defines a set of components each entity requires any from. 
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public class AnyAttribute : global::System.Attribute
{
    /// <summary>
    /// The types of the component.
    /// </summary>
    public Type[] ComponentTypes { get; }

    public AnyAttribute(params Type[] componentTypes)
    {
        ComponentTypes = componentTypes;
    }
}

/// <summary>
///     Defines a set of components none of the entities should have. 
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public class NoneAttribute : global::System.Attribute
{

    /// <summary>
    /// The types of the component.
    /// </summary>
    public Type[] ComponentTypes { get; }

    public NoneAttribute(params Type[] componentTypes)
    {
        ComponentTypes = componentTypes;
    }
}

/// <summary>
///     Defines an exclusive set of components an entity should have. 
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
public class ExclusiveAttribute : global::System.Attribute
{

    /// <summary>
    /// The types of the component.
    /// </summary>
    public Type[] ComponentTypes { get; }

    public ExclusiveAttribute(params Type[] componentTypes)
    {
        ComponentTypes = componentTypes;
    }
}