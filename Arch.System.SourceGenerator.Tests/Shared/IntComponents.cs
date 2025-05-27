namespace Arch.System.SourceGenerator.Tests;

#pragma warning disable CS0649 // Allow fields to be unassigned for testing purposes
internal interface IIntComponent
{
    int Value { get; }
}

internal struct IntComponentA : IIntComponent
{
    public int Value;

    readonly int IIntComponent.Value
    {
        get => Value;
    }
}

internal struct IntComponentB : IIntComponent
{
    public int Value;

    readonly int IIntComponent.Value
    {
        get => Value;
    }
}

internal struct IntComponentC : IIntComponent
{
    public int Value;

    readonly int IIntComponent.Value
    {
        get => Value;
    }
}

internal struct IntComponentD : IIntComponent
{
    public int Value;

    readonly int IIntComponent.Value
    {
        get => Value;
    }
}

#pragma warning restore CS0649
