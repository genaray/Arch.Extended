using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests basic query functionality.
/// </summary>
internal partial class AccessibilitySystem : BaseTestSystem
{
    public AccessibilitySystem(World world) : base(world) { }

    private int _number = 0;

    [Query]
    public void Default(IntComponentA _)
    {
        _number++;
    }

    [Query(Accessibility = QueryAccessibility.Public)]
    public void Public(IntComponentA _)
    {
        _number++;
    }

    [Query(Accessibility = QueryAccessibility.Internal)]
    public void Internal(IntComponentA _)
    {
        _number++;
    }

    [Query(Accessibility = QueryAccessibility.Private)]
    public void Private(IntComponentA _)
    {
        _number++;
    }

    [Query(Accessibility = QueryAccessibility.Protected)]
    public void Protected(IntComponentA _)
    {
        _number++;
    }

    [Query(Accessibility = QueryAccessibility.ProtectedInternal)]
    public void ProtectedInternal(IntComponentA _)
    {
        _number++;
    }

    public override void Setup()
    {
        World.Create(new IntComponentA());
    }

    public override void Update(in int t)
    {
        Assert.That(_number, Is.Zero);
        PublicQuery(World);
        Assert.That(_number, Is.EqualTo(1));
        InternalQuery(World);
        Assert.That(_number, Is.EqualTo(2));
        PrivateQuery(World);
        Assert.That(_number, Is.EqualTo(3));
        ProtectedQuery(World);
        Assert.That(_number, Is.EqualTo(4));
        ProtectedInternalQuery(World);
        Assert.That(_number, Is.EqualTo(5));
        DefaultQuery(World);
        Assert.That(_number, Is.EqualTo(6));
    }
}
