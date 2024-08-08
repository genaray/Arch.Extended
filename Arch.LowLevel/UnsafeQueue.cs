using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeStack{T}"/> represents a native unmanaged queue.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the queue.</typeparam>
[DebuggerTypeProxy(typeof(UnsafeQueueDebugView<>))]
public unsafe struct UnsafeQueue<T> : IEnumerable<T>, IDisposable where T : unmanaged
{
    private UnsafeArray<T> _queue;
    private int _capacity;
    private int _frontIndex;
    private int _count;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeQueue{T}"/>.
    /// </summary>
    /// <param name="capacity">Initial capacity of this queue.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public UnsafeQueue(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0.");
        }
        
        _queue = new UnsafeArray<T>(capacity);
        _capacity = capacity;
        _frontIndex =  _count = 0;
    }
    
    /// <summary>
    ///     The amount of items in the queue.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    ///     The total capacity of this queue.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _capacity;
    }

    /// <summary>
    ///     Enqueues a item.
    /// </summary>
    /// <param name="item">The item</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(T item)
    {
        if (Count == Capacity)
        {
            EnsureCapacity(_capacity * 2);
        }

        var itemOffset = (_frontIndex + _count) % _capacity;
        _queue[itemOffset] = item;
        _count++;
    }

    /// <summary>
    ///     Dequeues an item.
    /// </summary>
    /// <returns>The item</returns>
    /// <exception cref="InvalidOperationException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        var item = Peek();
        _frontIndex = (_frontIndex + 1) % _capacity;
        _count--;
        return item;
    }

    /// <summary>
    ///     Peeks at an item.
    /// </summary>
    /// <returns>The item.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        return ref _queue[_frontIndex];
    }
    
    /// <summary>
    ///     Trims this instance and releases memory in this process.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        var newCapacity = _count;
        SetCapacity(newCapacity);
    }

    /// <summary>
    ///     Ensures the capacity of this instance. 
    /// </summary>
    /// <param name="newCapacity">The new capacity.</param>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int newCapacity)
    {
        if (newCapacity <= _capacity)
        {
            return;
        }

        SetCapacity(newCapacity);
    }

    /// <summary>
    ///     Ensures the capacity of this instance. 
    /// </summary>
    /// <param name="newCapacity">The new capacity.</param>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCapacity(int newCapacity)
    {
        if (newCapacity < _count)
        {
            throw new ArgumentOutOfRangeException(nameof(newCapacity), "newCapacity cannot be smaller than _count");
        }

        var newBuffer = new UnsafeArray<T>(newCapacity);
        if (_count > 0)
        {
            var firstChunkCount = Math.Min(_count, _capacity - _frontIndex);
            var secondChunkCount = _count - firstChunkCount;

            // Copy elements in front->rear order to the new buffer
            if (firstChunkCount > 0)
            {
                UnsafeArray.Copy(ref _queue, _frontIndex, ref newBuffer, 0, firstChunkCount);
            }

            if (secondChunkCount > 0)
            {
                UnsafeArray.Copy(ref _queue, 0, ref newBuffer, firstChunkCount, secondChunkCount);
            }
        }
        
        _queue.Dispose();
        
        _queue = newBuffer;
        _capacity = newCapacity;
        _frontIndex = 0;
    }

    /// <summary>
    ///     Clears this instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _frontIndex = _count = 0;
    }

    /// <summary>
    ///     Disposes this instance. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _queue.Dispose();
        _capacity = _frontIndex  = _count = 0;
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeQueue{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(_queue, Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the list content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator<T> GetEnumerator()
    {
        return new UnsafeEnumerator<T>(_queue, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerable{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new UnsafeIEnumerator<T>(_queue, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new UnsafeIEnumerator<T>(_queue, Count);
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeStack{T}"/> to a string.
    /// </summary>
    /// <returns>The string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        var items = new StringBuilder();
        foreach (ref var item in this)
        {
            items.Append($"{item},");
        }
        items.Length--;
        return $"UnsafeQueue<{typeof(T).Name}>[{Count}]{{{items}}}";
    }
}

/// <summary>
///     A debug view for the <see cref="UnsafeQueue{T}"/>.
/// </summary>
/// <typeparam name="T">The unmanaged type.</typeparam>
internal class UnsafeQueueDebugView<T> where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly UnsafeQueue<T> _entity;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
        get
        {
            var items = new T[_entity.Count];
            _entity.AsSpan().CopyTo(items);
            return items;
        }
    }

    public UnsafeQueueDebugView(UnsafeQueue<T> entity) => _entity = entity;
}