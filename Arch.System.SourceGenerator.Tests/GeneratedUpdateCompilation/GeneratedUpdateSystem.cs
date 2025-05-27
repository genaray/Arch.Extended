using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests the auto-generated <see cref="Update(in int)"/> method.
/// </summary>
internal partial class GeneratedUpdateSystem : BaseTestSystem
{
    public GeneratedUpdateSystem(World world) : base(world) { }

    private int _number = 0;

    [Query]
    [All(typeof(IntComponentA))]
    public void AutoRunA()
    {
        Assert.That(_number, Is.EqualTo(0));
        _number++;
    }

    [Query]
    [All(typeof(IntComponentA))]
    public void AutoRunB()
    {
        Assert.That(_number, Is.EqualTo(1));
        _number++;
    }

    public override void Setup()
    {
        World.Create(new IntComponentA());
    }

    public override void Test()
    {
        base.Test();
        Assert.That(_number, Is.EqualTo(2));
    }
}
