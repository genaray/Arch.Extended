using System.Collections;
using static NUnit.Framework.Assert;
namespace Arch.LowLevel.Tests;

/// <summary>
///     Checks <see cref="UnsafeList{T}"/> related methods.
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

        // Check that adding past the end throws
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            list.Insert(5, 5);
        });

        // Add lots of items, to force capacity to grow
        var count = 10;
        for (var i = 0; i < count; i++)
            list.Insert(0, 0);

        CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3 }, list);
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

        Throws<ArgumentOutOfRangeException>(() =>
        {
            list.RemoveAt(-1);
        });
        Throws<ArgumentOutOfRangeException>(() =>
        {
            list.RemoveAt(10);
        });
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

        That(list.Remove(2), Is.True);
        That(list.Remove(4), Is.False);

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
        That(list.Contains(0), Is.EqualTo(false));
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
        That(list.IndexOf(0), Is.EqualTo(-1));
        That(list.IndexOf(4), Is.EqualTo(-1));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of ensuring capacity.
    /// </summary>
    [Test]
    public void UnsafeListEnsureCapacity()
    {
        using var list = new UnsafeList<int>(8);
        list.EnsureCapacity(16);
        list.Add(0);
        list.Add(1);
        
        That(list.Capacity, Is.EqualTo(16));
        That(list.IndexOf(0), Is.EqualTo(0));
        That(list.IndexOf(1), Is.EqualTo(1));

        // This should do nothing
        list.EnsureCapacity(list.Count);
        That(list.Capacity, Is.EqualTo(16));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of trimming capacity.
    /// </summary>
    [Test]
    public void UnsafeListTrimExcess()
    {
        using var list = new UnsafeList<int>(16);
        list.Add(0);
        list.Add(1);
        list.TrimExcess();
        
        That(list.Capacity, Is.EqualTo(2));
        That(list.IndexOf(0), Is.EqualTo(0));
        That(list.IndexOf(1), Is.EqualTo(1));
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
            That(++count, Is.EqualTo(item));
        }
        That(count, Is.EqualTo(3));
        
        // Ilist iterator
        count = 0;
        foreach (var item in list as IList<int>)
        {
            That(++count, Is.EqualTo(item));
        }
        That(count, Is.EqualTo(3));

        // non-generic enumerator
        count = 0;
        foreach (var item in ((IEnumerable)list))
        {
            That(++count, Is.EqualTo(item));
        }
        That(count, Is.EqualTo(3));
    }

    /// <summary>
    ///     Checks if the unsafe list enumerator can be reset
    /// </summary>
    [Test]
    public void UnsafeListEnumeratorReset()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        var enumerator = list.GetEnumerator();

        That(enumerator.MoveNext(), Is.True);
        That(enumerator.Current, Is.EqualTo(1));
        That(enumerator.MoveNext(), Is.True);
        That(enumerator.Current, Is.EqualTo(2));

        enumerator.Reset();

        var count = 1;
        foreach (var item in list)
        {
            That(count, Is.EqualTo(item));
            count++;
        }
    }

    [Test]
    public void UnsafeListFuzz()
    {
        using var list = new UnsafeList<int>(8);
        var truth = new List<int>();

        var rng = new Random(3462345);

        for (var i = 0; i < 1024; i++)
        {
            var index = rng.Next(0, list.Count);
            var value = rng.Next();
            switch (rng.Next(0, 5))
            {
                case 0:
                {
                    truth.Add(value);
                    list.Add(value);
                    break;
                }

                case 1:
                {
                    truth.Remove(value);
                    list.Remove(value);
                    break;
                }

                case 2 when list.Count > 0:
                {
                    truth.RemoveAt(index);
                    list.RemoveAt(index);
                    break;
                }

                case 3:
                {
                    truth.Insert(index, value);
                    list.Insert(index, value);
                    break;
                }

                case 4 when list.Count > 0:
                {
                    value = truth[index];
                    That(list, Does.Contain(value));
                    break;
                }
            }

            CollectionAssert.AreEqual(truth, list);
        }
    }
}