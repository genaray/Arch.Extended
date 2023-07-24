using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeStack{T}"/> represents a native unmanaged stack.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the stack.</typeparam>
public unsafe struct UnsafeStack<T> :  IEnumerable<T>, IDisposable where T : unmanaged  
{
    private const int DefaultCapacity = 4;
    
    /// <summary>
    ///     The stack pointer.
    /// </summary>
    private T* _stack;
    
    /// <summary>
    ///     Its capacity.
    /// </summary>
    private int _capacity;
    
    /// <summary>
    ///     Its count.
    /// </summary>
    private int _count;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeStack{T}"/>.
    /// </summary>
    /// <param name="capacity">The initial capacity that is being allocated.</param>
    public UnsafeStack(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0.");

        this._capacity = capacity;
        _count = 0;

#if NET6_0_OR_GREATER
        _stack = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * capacity));
#else
        _stack = (T*)Marshal.AllocHGlobal(sizeof(T) * capacity);
#endif
    }

    /// <summary>
    ///     The amount of items in the stack.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    ///     The total capacity of this stack.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _capacity;
    }

    /// <summary>
    ///     If this stack is full.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count == 0;
    }

    /// <summary>
    ///     If this stack is empty. 
    /// </summary>
    public bool IsFull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count >= _capacity;
    }

    /// <summary>
    ///     Pushes an item to the <see cref="UnsafeStack{T}"/>.
    /// </summary>
    /// <param name="value">The item.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T value)
    {
        if (_count >= _capacity)
        {
            EnsureCapacity(_capacity + 1);
        }

        *(_stack + _count) = value;
        _count++;
    }

    /// <summary>
    ///     Pops the first item of this <see cref="UnsafeStack{T}"/> and returns it.
    /// </summary>
    /// <returns>The item.</returns>
    /// <exception cref="InvalidOperationException">If the stack is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        _count--;
        return *(_stack + _count);
    }

    /// <summary>
    ///     Peeks at the first item of this <see cref="UnsafeStack{T}"/> and returns it. 
    /// </summary>
    /// <returns>The item.</returns>
    /// <exception cref="InvalidOperationException">If the stack is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        return *(_stack + _count - 1);
    }
    
    /// <summary>
    ///     Ensures the capacity of this <see cref="UnsafeStack{T}"/> instance and resizes it accordingly.
    /// </summary>
    /// <param name="min">The minimum amount of items ensured.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int min)
    {
        if (min <= _capacity)
        {
            return;
        }

        var newCapacity = _capacity * 2;
        if (newCapacity < min)
        {
            newCapacity = min;
        }

#if NET6_0_OR_GREATER
        var newStack = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * newCapacity));
#else
        var newStack = (T*)Marshal.AllocHGlobal(sizeof(T) * newCapacity);
#endif
        
        var tempPointer = _stack;
        var newStackPointer = newStack;

        while (tempPointer < _stack + _count)
        {
            *newStackPointer = *tempPointer;
            newStackPointer++;
            tempPointer++;
        }

        _capacity = newCapacity;
        _stack = newStack;
    }

    /// <summary>
    ///     Trims the capacity of this <see cref="UnsafeStack{T}"/> to release unused memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        var newCapacity = _count == 0 ? DefaultCapacity : _count;
        if (newCapacity >= _capacity)
        {
            return;
        }
        
#if NET6_0_OR_GREATER
        var newStack = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * newCapacity));
#else
            var newStack = (T*)Marshal.AllocHGlobal(sizeof(T) * newCapacity);
#endif
            
        var tempPointer = _stack;
        var newStackPointer = newStack;

        while (tempPointer < _stack + _count)
        {
            *newStackPointer = *tempPointer;
            newStackPointer++;
            tempPointer++;
        }

        _capacity = newCapacity;
        _stack = newStack;
    }

    /// <summary>
    ///     Clears this <see cref="UnsafeList{T}"/> instance.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _count = 0;
    }
    
    /// <summary>
    ///     Disposes this instance and releases its memory. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
#if NET6_0_OR_GREATER
        NativeMemory.Free(_stack);
#else
        Marshal.FreeHGlobal((IntPtr)_stack);
#endif
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeStack{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(_stack, Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the list content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeReverseEnumerator<T> GetEnumerator()
    {
        return new UnsafeReverseEnumerator<T>(_stack, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerable{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new ReverseEnumerator<T>(_stack, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new ReverseEnumerator<T>(_stack, Count);
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
        return $"UnsafeStack<{typeof(T).Name}>[{Count}]{{{items}}}";
    }
}