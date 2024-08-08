using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeStack{T}"/> represents a native unmanaged stack.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the stack.</typeparam>
[DebuggerTypeProxy(typeof(UnsafeStackDebugView<>))]
public unsafe struct UnsafeStack<T> :  IEnumerable<T>, IDisposable where T : unmanaged  
{
    private const int DefaultCapacity = 4;
    
    /// <summary>
    ///     The stack pointer.
    /// </summary>
    private UnsafeArray<T> _stack;
    
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
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than 0.");
        }

        _stack = new UnsafeArray<T>(capacity);
        _capacity = capacity;
        _count = 0;
    }

    /// <summary>
    ///     The amount of items in the stack.
    /// </summary>
    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    /// <summary>
    ///     The total capacity of this stack.
    /// </summary>
    public readonly int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _capacity;
    }

    /// <summary>
    ///     If this stack is empty.
    /// </summary>
    public readonly bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count == 0;
    }

    /// <summary>
    ///     If this stack is full. 
    /// </summary>
    public readonly bool IsFull
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
        if (Count == Capacity)
        {
            EnsureCapacity(_capacity * 2);
        }
        
        _stack[_count] = value;
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
        return _stack[_count];
    }

    /// <summary>
    ///     Peeks at the first item of this <see cref="UnsafeStack{T}"/> and returns it. 
    /// </summary>
    /// <returns>The item.</returns>
    /// <exception cref="InvalidOperationException">If the stack is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        return ref _stack[_count - 1];
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

        // Create new stack and copy elements
        var newStack = new UnsafeArray<T>(newCapacity);
        UnsafeArray.Copy(ref _stack, 0, ref newStack,0, _count);
        _stack.Dispose();

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
        
        // Create new stack and copy elements
        var newStack = new UnsafeArray<T>(newCapacity);
        UnsafeArray.Copy(ref _stack, 0, ref newStack,0, _count);
        _stack.Dispose();
        
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
        _stack.Dispose();
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeStack{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan()
    {
        return new Span<T>(_stack, Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the list content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly UnsafeReverseEnumerator<T> GetEnumerator()
    {
        return new UnsafeReverseEnumerator<T>(_stack, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerable{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new ReverseIEnumerator<T>(_stack, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return new ReverseIEnumerator<T>(_stack, Count);
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeStack{T}"/> to a string.
    /// </summary>
    /// <returns>The string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override string ToString()
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

/// <summary>
///     A debug view for the <see cref="UnsafeQueue{T}"/>.
/// </summary>
/// <typeparam name="T">The unmanaged type.</typeparam>
internal class UnsafeStackDebugView<T> where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly UnsafeStack<T> _entity;

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

    public UnsafeStackDebugView(UnsafeStack<T> entity) => _entity = entity;
}