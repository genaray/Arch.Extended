using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests basic query functionality.
/// </summary>
internal partial class BasicSystem : BaseTestSystem
{
    public BasicSystem(World world) : base(world) { }

    private int _number = 0;
    static private int _numberStatic = 0;

    [Query]
    public void Basic(IntComponentA _)
    {
        _number++;
        _numberStatic++;
    }

    [Query]
    public static void BasicStatic(IntComponentA _)
    {
        _numberStatic++;
    }

    public override void Setup()
    {
        World.Create(new IntComponentA());
    }

    public override void Update(in int t)
    {
        Assert.That(_number, Is.EqualTo(0));
        Assert.That(_numberStatic, Is.EqualTo(0));
        BasicQuery(World);
        Assert.That(_number, Is.EqualTo(1));
        Assert.That(_numberStatic, Is.EqualTo(1));
        BasicStaticQuery(World);
        Assert.That(_number, Is.EqualTo(1));
        Assert.That(_numberStatic, Is.EqualTo(2));
    }
}
