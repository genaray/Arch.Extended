using static NUnit.Framework.Assert;

namespace Arch.LowLevel.Tests;

/// <summary>
///     Checks <see cref="Resources{T}"/> and HashCode related methods.
/// </summary>
[TestFixture]
public class ResourcesTest
{

    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> is capable of adding <see cref="Handle{T}"/>s.
    /// </summary>
    [Test]
    public void ResourcesAddHandle()
    {
        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");
        var nextHandle = resources.Add("NextHandle");

        That(handle.Id, Is.EqualTo(0));
        That(nextHandle.Id, Is.EqualTo(1));
    }

    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> is capable of getting <see cref="Handle{T}"/>s.
    /// </summary>
    [Test]
    public void ResourcesGetHandle()
    {
        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");
        var nextHandle = resources.Add("NextHandle");

        // Check get
        var handleString = resources.Get(in handle);
        var nextHandleString = resources.Get(in nextHandle);

        That(handleString, Is.EqualTo("Handle"));
        That(nextHandleString, Is.EqualTo("NextHandle"));
    }

    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> is capable of removing <see cref="Handle{T}"/>s.
    /// </summary>
    [Test]
    public void ResourcesRemoveHandle()
    {

        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");
        var nextHandle = resources.Add("NextHandle");

        // Check remove
        resources.Remove(in handle);
        resources.Remove(in nextHandle);

        That(resources._ids.Count, Is.EqualTo(2));
        That(resources.Count, Is.EqualTo(0));
    }

    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> is capable of removing <see cref="Handle{T}"/>s.
    /// </summary>
    [Test]
    public void ResourcesRecycleHandle()
    {
        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");
        var nextHandle = resources.Add("NextHandle");

        // Check remove
        resources.Remove(in handle);
        resources.Remove(in nextHandle);

        var newHandle = resources.Add("NewString");
        That(newHandle.Id, Is.EqualTo(0));
        That(resources.Count, Is.EqualTo(1));
    }
}