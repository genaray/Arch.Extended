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
    ///     Checks if <see cref="Resources{T}"/> is capable of adding many more <see cref="Handle{T}"/>s than the capacity
    /// </summary>
    [Test]
    public void ResourcesAddManyHandles()
    {
        const int count = 10000;

        using var resources = new Resources<string>(capacity: 3);

        var handles = new List<Handle<string>>();
        for (var i = 0; i < count; i++)
            handles.Add(resources.Add(i.ToString()));

        resources.TrimExcess();

        That(resources.Count, Is.EqualTo(count));

        for (var i = 0; i < handles.Count; i++)
            That(resources.Get(handles[i]), Is.EqualTo(i.ToString()));
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
    
    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> is capable of validating a <see cref="Handle{T}"/>.
    /// </summary>
    [Test]
    public void ResourcesHandleValid()
    {
        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");
        Handle<string> someHandle = Handle<string>.NULL;
        
        That(resources.IsValid(handle), Is.EqualTo(true));
        That(resources.IsValid(someHandle), Is.EqualTo(false));
    }

    /// <summary>
    ///     Checks if <see cref="Resources{T}"/> throws after Dispose
    /// </summary>
    [Test]
    public void ResourcesDispose()
    {
        // Check add
        var resources = new Resources<string>(IntPtr.Size, capacity: 64);
        var handle = resources.Add("Handle");

        // Check get
        That(resources.Get(in handle), Is.EqualTo("Handle"));
        
        resources.Dispose();

        That(resources.Count, Is.EqualTo(0));

        // Check that get fails
        Throws<NullReferenceException>(() =>
        {
            resources.Get(in handle);
        });
    }
}