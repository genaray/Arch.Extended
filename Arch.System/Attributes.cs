namespace Arch.System;

/// <summary>
///     Marks a method to generate a high performance query for it. 
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Method)]
public class QueryAttribute : global::System.Attribute
{
    /// <summary>
    /// If set to true, Query will be run in parallel.
    /// </summary>
    public bool Parallel { get; set; }
}

/// <summary>
///     Marks a parameter as "data". This will be taken into account during source generation and will still be passed as a parameter in the query method.
///     Is not treated as an entity component.
/// </summary>
[global::System.AttributeUsage(global::System.AttributeTargets.Parameter)]
public class DataAttribute : global::System.Attribute
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