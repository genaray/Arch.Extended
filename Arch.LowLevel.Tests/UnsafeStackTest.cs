using System.Collections;

namespace Arch.LowLevel.Tests;
using static NUnit.Framework.Assert;

/// <summary>
///     Checks <see cref="UnsafeStack{T}"/> related methods.
/// </summary>
[TestFixture]
public class UnsafeStackTest
{
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> checks for invalid capacity on construction.
    /// </summary>
    [Test]
    public void UnsafeStackInvalidCapacity()
    {
        Throws<ArgumentOutOfRangeException>(() => new UnsafeStack<int>(-9));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of adding items.
    /// </summary>
    [Test]
    public void UnsafeStackAdd()
    {
        using var stack = new UnsafeStack<int>(8);

        That(stack.IsFull, Is.False);
        That(stack.IsEmpty, Is.True);

        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        That(stack.IsFull, Is.False);
        That(stack.IsEmpty, Is.False);

        That(stack.Count, Is.EqualTo(3));
        That(stack.Peek(), Is.EqualTo(3));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is can be converted to a span
    /// </summary>
    [Test]
    public void UnsafeStackAsSpan()
    {
        using var stack = new UnsafeStack<int>(8);

        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        var span = stack.AsSpan();

        stack.Pop();
        stack.Push(4);

        That(span.Length, Is.EqualTo(3));

        CollectionAssert.AreEqual(span.ToArray(), new[] { 1, 2, 4 });
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of adding items even past the initial capacity
    /// </summary>
    [Test]
    public void UnsafeStackAddBeyondCapacity()
    {
        using var stack = new UnsafeStack<int>(4);
        That(stack.Capacity, Is.EqualTo(4));

        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Push(4);
        That(stack.IsFull, Is.True);
        stack.Push(5);
        stack.Push(6);
        stack.Push(7);

        That(stack.Count, Is.EqualTo(7));
        That(stack.Peek(), Is.EqualTo(7));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}.EnsureCapacity"/> expands capacity
    /// </summary>
    [Test]
    public void UnsafeStackEnsureCapacityExpands()
    {
        using var stack = new UnsafeStack<int>(10);
        That(stack.Capacity, Is.EqualTo(10));

        stack.EnsureCapacity(11);

        That(stack.Capacity, Is.GreaterThanOrEqualTo(11));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}.EnsureCapacity"/> cannot shrink the capacity
    /// </summary>
    [Test]
    public void UnsafeStackEnsureCapacityCannotShrink()
    {
        using var stack = new UnsafeStack<int>(10);
        That(stack.Capacity, Is.EqualTo(10));

        stack.EnsureCapacity(1);

        That(stack.Capacity, Is.EqualTo(10));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}.EnsureCapacity"/> can add a massive amount of new capacity
    /// </summary>
    [Test]
    public void UnsafeStackEnsureCapacityExpandsALot()
    {
        using var stack = new UnsafeStack<int>(10);
        That(stack.Capacity, Is.EqualTo(10));

        stack.EnsureCapacity(10000);

        That(stack.Capacity, Is.GreaterThanOrEqualTo(10000));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}.TrimExcess"/> can remove unused capacity
    /// </summary>
    [Test]
    public void UnsafeStackTrimExcessShrinks()
    {
        using var stack = new UnsafeStack<int>(10);
        That(stack.Capacity, Is.EqualTo(10));

        stack.TrimExcess();

        That(stack.Capacity, Is.LessThan(10));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}.TrimExcess"/> does not expand
    /// </summary>
    [Test]
    public void UnsafeStackTrimExcessNeverExpands()
    {
        using var stack = new UnsafeStack<int>(2);
        
        stack.TrimExcess();

        That(stack.Capacity, Is.LessThanOrEqualTo(2));
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of being cleared.
    /// </summary>
    [Test]
    public void UnsafeStackClear()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        That(stack.Count, Is.EqualTo(3));
        That(stack.Peek(), Is.EqualTo(3));

        stack.Clear();

        That(stack.Count, Is.EqualTo(0));
        Throws<InvalidOperationException>(() => stack.Peek());
        Throws<InvalidOperationException>(() => stack.Pop());
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of peeking itemss.
    /// </summary>
    [Test]
    public void UnsafeStackPeek()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);

        That(stack.Peek(), Is.EqualTo(2));
        stack.Push(3);
        That(stack.Peek(), Is.EqualTo(3));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of popping itemss.
    /// </summary>
    [Test]
    public void UnsafeStackPop()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        
        That(stack.Pop(), Is.EqualTo(3));
        That(stack.Pop(), Is.EqualTo(2));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of iterating with its enumerators.
    /// </summary>
    [Test]
    public void UnsafeStackEnumerator()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Ref iterator
        var count = 0;
        foreach (ref var item in stack)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
    }

    /// <summary>
    ///     Checks if the stack enumerator can be reset
    /// </summary>
    [Test]
    public void UnsafeStackEnumeratorReset()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        var enumerator = stack.GetEnumerator();

        True(enumerator.MoveNext());
        That(enumerator.Current, Is.EqualTo(3));
        True(enumerator.MoveNext());
        That(enumerator.Current, Is.EqualTo(2));

        enumerator.Reset();

        var count = 3;
        foreach (var item in stack)
        {
            That(count, Is.EqualTo(item));
            count--;
        }
    }

    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of iterating with its enumerators.
    /// </summary>
    [Test]
    public void UnsafeStackIEnumerableTEnumerator()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        var enumerable = (IEnumerable<int>)stack;

        var count = 0;
        foreach (var item in enumerable)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
    }

    /// <summary>
    ///     Checks if the stack enumerator can be reset
    /// </summary>
    [Test]
    public void UnsafeStackIEnumerableEnumeratorReset()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        var enumerator = ((IEnumerable)stack).GetEnumerator();

        True(enumerator.MoveNext());
        That(enumerator.Current, Is.EqualTo(3));
        True(enumerator.MoveNext());
        That(enumerator.Current, Is.EqualTo(2));

        enumerator.Reset();

        var count = 3;
        foreach (var item in stack)
        {
            That(count, Is.EqualTo(item));
            count--;
        }
    }
}