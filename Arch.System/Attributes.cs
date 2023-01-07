namespace Arch.System.SourceGenerator;

[global::System.AttributeUsage(global::System.AttributeTargets.Method)]
public class UpdateAttribute : global::System.Attribute
{
}

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