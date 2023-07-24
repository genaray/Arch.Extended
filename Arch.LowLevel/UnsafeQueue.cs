using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeStack{T}"/> represents a native unmanaged queue.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the queue.</typeparam>
public unsafe struct UnsafeQueue<T> : IEnumerable<T>, IDisposable where T : unmanaged
{
    private T* _queue;
    private int _capacity;
    private int _frontIndex;
    private int _count;

    public UnsafeQueue(int capacity)
    {
#if NET6_0_OR_GREATER
        _queue = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * capacity));
#else
        _queue = (T*)Marshal.AllocHGlobal(sizeof(T) * capacity);
#endif
        this._capacity = capacity;
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
        if (_count == _capacity)
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
    public T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        return _queue[_frontIndex];
    }
    
    /// <summary>
    ///     Trims this instance and releases memory in this process.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        var newCapacity = _count;
        EnsureCapacity(newCapacity);
    }

    /// <summary>
    ///     Ensures the capacity of this instance. 
    /// </summary>
    /// <param name="newCapacity">The new capacity.</param>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int newCapacity)
    {
        if (newCapacity < _count)
        {
            throw new ArgumentException("New capacity must be greater than or equal to the current count.");
        }

        if (newCapacity == _capacity)
        {
            return;
        }
        
#if NET6_0_OR_GREATER
        var newBuffer = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * newCapacity));
#else
        var newBuffer = (T*)Marshal.AllocHGlobal(sizeof(T) * newCapacity);
#endif
        
        if (_count > 0)
        {
            var firstChunkCount = Math.Min(_count, _capacity - _frontIndex);
            var secondChunkCount = _count - firstChunkCount;

            // Copy elements in front->rear order to the new buffer
            if (firstChunkCount > 0)
            {
                Buffer.MemoryCopy(_queue + _frontIndex, newBuffer, sizeof(T) * firstChunkCount, sizeof(T) * firstChunkCount);
            }

            if (secondChunkCount > 0)
            {
                Buffer.MemoryCopy(_queue, newBuffer + firstChunkCount, sizeof(T) * secondChunkCount, sizeof(T) * secondChunkCount);
            }
        }
        
#if NET6_0_OR_GREATER
        NativeMemory.Free(_queue);
#else
        Marshal.FreeHGlobal((IntPtr)_queue);
#endif
        
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
 
#if NET6_0_OR_GREATER
        NativeMemory.Free(_queue);
#else
        Marshal.FreeHGlobal((IntPtr)_queue);
#endif
        _capacity = _frontIndex  = _count = 0;
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeQueue{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
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
        return new Enumerator<T>(_queue, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator<T>(_queue, Count);
    }
}
