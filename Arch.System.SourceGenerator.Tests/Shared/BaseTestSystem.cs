using Arch.Core;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
/// Provides a base class for test systems. This must be included in the compilation to ensure that the system is generated correctly.
/// </summary>
/// <param name="world">The world instance to which the system will be attached.</param>
internal abstract class BaseTestSystem : BaseSystem<World, int>
{
    protected BaseTestSystem(World world) : base(world) { }

    /// <summary>
    /// Sets up the system for testing. Create entities, components, and any other necessary state.
    /// </summary>
    public abstract void Setup();

    /// <summary>
    /// Runs the test logic for the system. By default, it simply calls the update pipeline.
    /// </summary>
    public virtual void Test()
    {
        BeforeUpdate(0);
        Update(0);
        AfterUpdate(0);
    }
}
