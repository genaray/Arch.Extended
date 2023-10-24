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
        using var queue = new UnsafeQueue<int>(8);

        for (var i = 0; i < 20; i++)
            queue.Enqueue(i);
        
        That(queue, Has.Count.EqualTo(20));
        That(queue.Peek(), Is.EqualTo(0));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of peeking itemss.
    /// </summary>
    [Test]
    public void UnsafeQueuePeek()
    {
        using var queue = new UnsafeQueue<int>(8);
        queue.Enqueue(1);
        queue.Enqueue(2);

        That(queue.Peek(), Is.EqualTo(1));
        queue.Enqueue(3);
        That(queue.Peek(), Is.EqualTo(1));
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeStack{T}"/> is capable of popping itemss.
    /// </summary>
    [Test]
    public void UnsafeQueueDequeue()
    {
        using var queue = new UnsafeQueue<int>(8);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        
        That(queue.Dequeue(), Is.EqualTo(1));
        That(queue.Dequeue(), Is.EqualTo(2));
        That(queue.Dequeue(), Is.EqualTo(3));

        Throws<InvalidOperationException>(() =>
        {
            queue.Dequeue();
        });

        Throws<InvalidOperationException>(() =>
        {
            queue.Peek();
        });
    }

    [Test]
    public void UnsafeQueueClear()
    {
        using var queue = new UnsafeQueue<int>(8);

        for (var i = 0; i < 20; i++)
            queue.Enqueue(i);

        That(queue, Has.Count.EqualTo(20));

        queue.Clear();

        That(queue, Is.Empty);
    }
    
    /// <summary>
    ///     Checks if <see cref="UnsafeList{T}"/> is capable of iterating with its enumerators.
    /// </summary>
    [Test]
    public void UnsafeQueueEnumerator()
    {
        using var queue = new UnsafeQueue<int>(8);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Ref iterator
        var count = 0;
        foreach (ref var item in queue)
        {
            count++;
        }
        That(count, Is.EqualTo(3));
    }

    [Test]
    public void UnsafeQueueInvalidConstruction()
    {
        Throws<ArgumentOutOfRangeException>(() =>
        {
            new UnsafeQueue<int>(-8);
        });
    }
}