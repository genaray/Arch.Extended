using System;
using Arch.Core;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
///     Runs tests for systems in each compilation.
///     Note that the compilation is shared across all tests, so the systems are not isolated.
///     As a result, the tests may not be completely independent. However, they are easier to debug.
///     Separately, the same tests are run in isolation in the Arch.System.SourceGenerator.Tests project.
/// </summary>
[TestFixture]
internal sealed class SystemsTest
{
    /// <summary>
    ///     Tests a system by creating it and running its update method.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the system to test, which must inherit from BaseSystem and must have a constructor that takes a World parameter.
    /// </typeparam>
    private static void TestSystem<T>() where T : BaseTestSystem
    {
        using var world = World.Create();
        var system = Activator.CreateInstance(typeof(T), world) as T;
        Assert.That(system, Is.Not.Null,
            $"System instance {typeof(T).Name} should not be null. Ensure it has a constructor that takes a single World param.");
        system.Setup();
        system.Test();
    }

    [Test]
    public void BasicCompilation()
    {
        TestSystem<BasicSystem>();
    }

    [Test]
    public void AttributeQueryCompilation()
    {
        TestSystem<AttributeQuerySystem>();
    }

    [Test]
    public void ParamQueryCompilation()
    {
        TestSystem<ParamQuerySystem>();
    }

    [Test]
    public void DataParamCompilation()
    {
        TestSystem<DataParamSystem>();
    }

    [Test]
    public void GeneratedUpdateCompilation()
    {
        TestSystem<GeneratedUpdateSystem>();
    }
}
