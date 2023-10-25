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
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of adding items.
    /// </summary>
    [Test]
    public void UnsafeListAdd()
    {
        using var list = new UnsafeList<int>(8);
        That(list.IsReadOnly, Is.False);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        
        That(list.Count, Is.EqualTo(3));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> GetHashCode is different for different lists
    /// </summary>
    [Test]
    public void UnsafeListGetHashCode()
    {
        using var list1 = new UnsafeList<int>(8);
        using var list2 = new UnsafeList<int>(8);

        That(list1.GetHashCode(), Is.Not.EqualTo(list2.GetHashCode()));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> can access items by index
    /// </summary>
    [Test]
    public void UnsafeListRefIndex()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(7);

        ref var item0 = ref list[0];
        That(item0, Is.EqualTo(7));
        item0 = 11;
        That(list[0], Is.EqualTo(11));

#if DEBUG
        Throws<IndexOutOfRangeException>(() => { var x = list[-1]; });
        Throws<IndexOutOfRangeException>(() => { var x = list[2]; });
#endif
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> can access items by index
    /// </summary>
    [Test]
    public void UnsafeListIndex()
    {
        using var unsafelist = new UnsafeList<int>(8);
        unsafelist.Add(7);

        var list = (IList<int>)unsafelist;

        That(list[0], Is.EqualTo(7));
        list[0] = 11;
        That(list[0], Is.EqualTo(11));

#if DEBUG
        Throws<IndexOutOfRangeException>(() => { var x = list[-1]; });
        Throws<IndexOutOfRangeException>(() => { var x = list[2]; });
#endif
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of being copied to an array.
    /// </summary>
    [Test]
    public void UnsafeListCopyTo()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        var arr = new int[10];

        // Basic copy into the array
        list.CopyTo(arr, 3);
        CollectionAssert.AreEqual(new[] { 0, 0, 0, 1, 2, 3, 0, 0, 0, 0 }, arr);

        // Copy into a bad index
        Throws<IndexOutOfRangeException>(() => { list.CopyTo(arr, -3); });
        Throws<IndexOutOfRangeException>(() => { list.CopyTo(arr, arr.Length + 1); });

        // Copy into an index near the end, so there's not enough space
        Throws<ArgumentException>(() => list.CopyTo(arr, 8));

        // Check that copying into an array from an empty list does nothing
        list.Clear();
        var arr2 = arr.ToArray();
        list.CopyTo(arr, 0);
        CollectionAssert.AreEqual(arr, arr2);
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of being cleared.
    /// </summary>
    [Test]
    public void UnsafeListClear()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Clear();
        That(list, Is.Empty);
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is can be converted into a span.
    /// </summary>
    [Test]
    public void UnsafeListAsSpan()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list.AsSpan().ToArray());
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> equality works as expected
    /// </summary>
    [Test]
    public void UnsafeListEquality()
    {
        using var list1 = new UnsafeList<int>(8);
        list1.Add(1);
        list1.Add(2);
        list1.Add(3);

        using var list2 = new UnsafeList<int>(8);
        list2.Add(1);
        list2.Add(2);
        list2.Add(3);

        That(list1 == list2, Is.False);
        That(list1 != list2, Is.True);

        var list1a = list1;
        That(list1 == list1a, Is.True);
        That(list1 != list1a, Is.False);

        That(list1.Equals((object)list2), Is.False);
        That(list1.Equals((object)list1), Is.True);
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
    public void UnsafeListAsIListEnumeratorReset()
    {
        using var list = new UnsafeList<int>(8);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        using var enumerator = ((IList<int>)list).GetEnumerator();

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