namespace Arch.LowLevel.Tests;
using static NUnit.Framework.Assert;

/// <summary>
///     Checks <see cref="UnsafeArray{T}"/> related methods.
/// </summary>
[TestFixture]
public class UnsafeArrayTest
{
    /// <summary>
    ///     Checks if <see cref="UnsafeArray{T}"/> is capable of allocating space and adding items.
    /// </summary>
    [Test]
    public void UnsafeArrayCreate()
    {
        using var array = new UnsafeArray<int>(3);
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;
        
        That(array.Count, Is.EqualTo(3));
    }
}