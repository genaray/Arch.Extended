namespace Arch.LowLevel.Tests;
using static NUnit.Framework.Assert;

/// <summary>
///     Checks <see cref="UnsafeStack{T}"/> related methods.
/// </summary>
[TestFixture]
public class UnsafeQueueTest
{
   
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of adding itemss.
    /// </summary>
    [Test]
    public void UnsafeQueueEnqueue()
    {
        using var stack = new UnsafeQueue<int>(8);
        stack.Enqueue(1);
        stack.Enqueue(2);
        stack.Enqueue(3);
        
        That(stack.Count, Is.EqualTo(3));
        That(stack.Peek(), Is.EqualTo(1));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of peeking itemss.
    /// </summary>
    [Test]
    public void UnsafeQueuePeek()
    {
        using var stack = new UnsafeQueue<int>(8);
        stack.Enqueue(1);
        stack.Enqueue(2);

        That(stack.Peek(), Is.EqualTo(1));
        stack.Enqueue(3);
        That(stack.Peek(), Is.EqualTo(1));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of popping itemss.
    /// </summary>
    [Test]
    public void UnsafeQueueDequeue()
    {
        using var stack = new UnsafeQueue<int>(8);
        stack.Enqueue(1);
        stack.Enqueue(2);
        stack.Enqueue(3);
        
        That(stack.Dequeue(), Is.EqualTo(1));
        That(stack.Dequeue(), Is.EqualTo(2));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of iterating with its enumerators.
    /// </summary>
    [Test]
    public void UnsafeQueueEnumerator()
    {
        using var stack = new UnsafeQueue<int>(8);
        stack.Enqueue(1);
        stack.Enqueue(2);
        stack.Enqueue(3);

        // Ref iterator
        var count = 0;
        foreach (ref var item in stack)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
    }
}