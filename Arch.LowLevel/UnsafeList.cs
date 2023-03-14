using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeList{T}"/> represents a native unmanaged list.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the list.</typeparam>
public unsafe struct UnsafeList<T> : IDisposable where T : unmanaged
{
    /// <summary>
    ///     The array pointer.
    /// </summary>
    private T* _array;
    
    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeList{T}"/>.
    /// </summary>
    /// <param name="capacity">The initial capacity that is being allocated.</param>
    public UnsafeList(int capacity = 8)
    {
        Count = 0;
        _array = (T*)Marshal.AllocHGlobal(sizeof(T) * capacity);
    }

    /// <summary>
    ///     The amount of items in the list.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get; 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    /// <summary>
    ///     Adds an item to the list.
    /// </summary>
    /// <param name="item">The item.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T item)
    {
        this[Count] = item;
        Count++;
    }

    /// <summary>
    ///     Removes an item from the list at a given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Throws when the index is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(in int index)
    {
        if ((uint)index >= (uint)Count) 
        {
            throw new ArgumentOutOfRangeException();
        }
        
        Count--;
        if (index < Count) 
        {
            Buffer.MemoryCopy(_array+(index+1), _array+index,Count-index,Count-index);
        }
        this[Count] = default;
    }
    
    /// <summary>
    ///     Acesses an item at the index of the list. 
    /// </summary>
    /// <param name="i"></param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array[i];
    }

    /// <summary>
    ///     Disposes this instance and releases its memory. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr)_array);
    }
}