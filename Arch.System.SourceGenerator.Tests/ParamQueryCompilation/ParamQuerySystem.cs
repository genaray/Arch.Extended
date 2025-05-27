using System;
using System.Collections.Generic;
using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Tests queries using parameters.
/// </summary>
internal partial class ParamQuerySystem : BaseTestSystem
{
    public ParamQuerySystem(World world) : base(world) { }

    [Query]
    public static void IncrementA(ref IntComponentA a)
    {
        a.Value++;
    }

    [Query]
    public static void IncrementOnlyAWithB(ref IntComponentA a, in IntComponentB _)
    {
        a.Value++;
    }

    [Query]
    [None(typeof(IntComponentC))]
    public static void IncrementANotC(ref IntComponentA a)
    {
        a.Value++;
    }

    [Query]
    public static void IncrementAAndB(ref IntComponentA a, ref IntComponentB b)
    {
        a.Value++;
        b.Value++;
    }

    private (Entity, Dictionary<Type, int> ComponentValues)[] _expectedComponentValues
        = Array.Empty<(Entity, Dictionary<Type, int> ComponentValues)>();

    public override void Setup()
    {
        _expectedComponentValues = new[]
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

        IncrementOnlyAWithBQuery(World);
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentA)]++;
        TestExpectedValues();

        IncrementANotCQuery(World);
        _expectedComponentValues[0].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        TestExpectedValues();

        IncrementAAndBQuery(World);
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[2].ComponentValues[typeof(IntComponentB)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentA)]++;
        _expectedComponentValues[3].ComponentValues[typeof(IntComponentB)]++;
        TestExpectedValues();
    }
}
