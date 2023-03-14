using System.Collections;
using static NUnit.Framework.Assert;
namespace Arch.LowLevel.Tests;

/// <summary>
///     Checks <see cref="Resources{T}"/> and HashCode related methods.
/// </summary>
[TestFixture]
public class UnsafeListTest
{
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of adding itemss.
    /// </summary>
    [Test]
    public void UnsafeListAdd()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        That(list.Count, Is.EqualTo(3));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of adding items at a given index.
    /// </summary>
    [Test]
    public void UnsafeListInsertAt()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(3);
        list.Insert(1,2);
        
        That(list.Count, Is.EqualTo(3));
        That(list[0], Is.EqualTo(1));
        That(list[1], Is.EqualTo(2));
        That(list[2], Is.EqualTo(3));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of removing itemss.
    /// </summary>
    [Test]
    public void UnsafeListRemoveAt()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        list.RemoveAt(0);
        list.RemoveAt(2);
        
        That(list.Count, Is.EqualTo(1));
        That(list[0], Is.EqualTo(2));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of removing items by value.
    /// </summary>
    [Test]
    public void UnsafeListRemove()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        list.Remove(2);
        
        That(list.Count, Is.EqualTo(2));
        That(list[1], Is.EqualTo(3));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of checking for a contained value.
    /// </summary>
    [Test]
    public void UnsafeListContains()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        That(list.Count, Is.EqualTo(3));
        That(list.Contains(2), Is.EqualTo(true));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of checking for a contained value.
    /// </summary>
    [Test]
    public void UnsafeListIndexOf()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        That(list.Count, Is.EqualTo(3));
        That(list.IndexOf(2), Is.EqualTo(1));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of iterating with its enumerators.
    /// </summary>
    [Test]
    public void UnsafeListEnumerator()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Ref iterator
        var count = 0;
        foreach (ref var item in list)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
        
        // Ilist iterator
        count = 0;
        foreach (var item in list as IList<int>)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
    }
}