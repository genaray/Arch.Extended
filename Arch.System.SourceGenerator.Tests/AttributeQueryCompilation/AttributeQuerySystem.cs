using System;
using System.Collections.Generic;
using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests queries with different attribute combinations.
/// </summary>
internal partial class AttributeQuerySystem : BaseTestSystem
{
    public AttributeQuerySystem(World world) : base(world) { }

    [Query]
    [All(typeof(IntComponentA))]
    public void IncrementA(Entity e)
    {
        ref var a = ref World.Get<IntComponentA>(e);
        a.Value++;
    }

    [Query]
    [Any(typeof(IntComponentA), typeof(IntComponentB))]
    public void IncrementAOrB(Entity e)
    {
        ref var a = ref World.TryGetRef<IntComponentA>(e, out bool aExists);
        ref var b = ref World.TryGetRef<IntComponentB>(e, out bool bExists);

        if (aExists)
        {
            a.Value++;
        }

        if (bExists)
        {
            b.Value++;
        }
    }

    [Query]
    [Any(typeof(IntComponentA), typeof(IntComponentB))]
    [None(typeof(IntComponentC))]
    public void IncrementAOrBNotC(Entity e)
    {
        ref var a = ref World.TryGetRef<IntComponentA>(e, out bool aExists);
        ref var b = ref World.TryGetRef<IntComponentB>(e, out bool bExists);

        if (aExists)
        {
            a.Value++;
        }

        if (bExists)
        {
            b.Value++;
        }
    }

    [Query]
    [All(typeof(IntComponentA), typeof(IntComponentB))]
    public void IncrementAAndB(Entity e)
    {
        ref var a = ref World.Get<IntComponentA>(e);
        a.Value++;
        ref var b = ref World.Get<IntComponentB>(e);
        b.Value++;
    }

    [Query]
    [All(typeof(IntComponentA))]
    [None(typeof(IntComponentB))]
    public void IncrementANotB(Entity e)
    {
        ref var a = ref World.Get<IntComponentA>(e);
        a.Value++;
    }

    [Query]
    [Exclusive(typeof(IntComponentA), typeof(IntComponentB))]
    public void IncrementAAndBExclusive(Entity e)
    {
        ref var a = ref World.Get<IntComponentA>(e);
        a.Value++;
        ref var b = ref World.Get<IntComponentB>(e);
        b.Value++;
    }

    private (Entity Entity, Dictionary<Type, int> ComponentValues)[]
        _expectedComponentValues = Array.Empty<(Entity, Dictionary<Type, int>)>();

    public override void Setup()
    {
        _expectedComponentValues = new []
        {
            (World.Create(new IntComponentA()),
                new Dictionary<Type, int> { { typeof(IntComponentA), 0 } }),
            (World.Create(new IntComponentB()),
                new Dictionary<Type, int> { { typeof(IntComponentB), 0 } }),
            (World.Create(new IntComponentA(), new IntComponentB()),
                new Dictionary<Type, int> { { typeof(IntComponentA), 0 }, { typeof(IntComponentB), 0 } }),
            (World.Create(new IntComponentA(), new IntComponentB(), new IntComponentC()),
                new Dictionary<Type, int> { { typeof(IntComponentA), 0 }, { typeof(IntComponentB), 0 }, { typeof(IntComponentC), 0 } })
        };
    }

    private void TestExpectedValues()
    {
        foreach (var (e, values) in _expectedComponentValues)
        {
            foreach (var (type, expectedValue) in values)
            {
                var component = World.Get(e, type) as IIntComponent;
                Assert.That(component, Is.Not.Null);
                Assert.That(component.Value, Is.EqualTo(expectedValue));
            }
        }
    }

    public override void Update(in int t)
    {
        TestExpectedValues();

        IncrementAQuery(World);
        _expectedComponentValues[0].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentA)]++;
        TestExpectedValues();

        IncrementAOrBQuery(World);
        _expectedComponentValues[0].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[1].ComponentValues[typeof(IntComponentB)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentB)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentB)]++;
        TestExpectedValues();

        IncrementAOrBNotCQuery(World);
        _expectedComponentValues[0].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[1].ComponentValues[typeof(IntComponentB)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentB)]++;
        TestExpectedValues();

        IncrementAAndBQuery(World);
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentB)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentB)]++;
        TestExpectedValues();

        IncrementANotBQuery(World);
        _expectedComponentValues[0].ComponentValues[typeof(IntComponentA)]++;
        TestExpectedValues();

        IncrementAAndBExclusiveQuery(World);
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentB)]++;
        TestExpectedValues();
    }
}
