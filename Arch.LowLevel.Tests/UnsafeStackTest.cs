namespace Arch.LowLevel.Tests;
using static NUnit.Framework.Assert;

/// <summary>
///     Checks <see cref="UnsafeStack{T}"/> related methods.
/// </summary>
[TestFixture]
public class UnsafeStackTest
{
   
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of adding itemss.
    /// </summary>
    [Test]
    public void UnsafeStackAdd()
    {
        using var stack = new UnsafeStack<int>(8);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        
        That(stack.Count, Is.EqualTo(3));
        That(stack.Peek(), Is.EqualTo(3));
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
}