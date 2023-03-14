using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Arch.LowLevel;

/// <summary>
///     The struct <see cref="UnsafeList{T}"/> represents a native unmanaged list.
///     Can easily be stored in unmanaged structs. 
/// </summary>
/// <typeparam name="T">The generic type stored in the list.</typeparam>
[DebuggerTypeProxy(typeof(UnsafeListDebugView<>))]
public unsafe struct UnsafeList<T> : IList<T>, IDisposable where T : unmanaged
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
        Capacity = capacity;
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
    ///     The total capacity of this list.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get; 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    /// <summary>
    ///     If its readonly.
    /// </summary>
    public bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     Adds an item to the list.
    /// </summary>
    /// <param name="item">The item.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        this[Count] = item;
        Count++;
    }
    
    /// <summary>
    ///     Inserts an item at the given index. 
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item)
    {
        // Inserting to end of the list is legal.
        if ((uint)index > (uint)Count)
        {
            throw new ArgumentOutOfRangeException(); 
        }
        
        // Resize if the list is actually full
        if(Capacity == Count+1) {
            EnsureCapacity(Capacity + 1);
        }

        if(index < Count) {
            Buffer.MemoryCopy(_array+index, _array+index+1,Capacity-Count,Capacity-Count);
        }
        
        _array[index] = item;
        Count++;
    }


    /// <summary>
    ///     Removes an item from the list at a given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Throws when the index is out of range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        if ((uint)index > (uint)Count) 
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
    ///     Removes the item by its value and returns true or false.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>True if the operation was sucessfull, false if it was not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        
        RemoveAt(index);
        return true;
    }
    
    /// <summary>
    ///     Checks if the item is containted in this <see cref="UnsafeList{T}"/> instance and returns its index.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>Its index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(T item)
    {
        for(var i = 0; i < Count; i++) 
        {
            if(EqualityComparer<T>.Default.Equals(_array[i], item)) 
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    ///     Checks if the item is containted in this <see cref="UnsafeList{T}"/> instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>True if it exists, otherwhise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        for(var i = 0; i < Count; i++) 
        {
            if(EqualityComparer<T>.Default.Equals(_array[i], item)) 
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     Copies all items from this <see cref="UnsafeList{T}"/> to the specified array.
    /// </summary>
    /// <param name="array">The array to copy to.</param>
    /// <param name="arrayIndex">The index to start with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        if(Count == 0) return;
        fixed(T* arrayPtr = array)
        {
            Buffer.MemoryCopy(_array, arrayPtr+arrayIndex, array.Length * sizeof(T), Count * sizeof(T));
        }
    }

    /// <summary>
    ///     Ensures the capacity of this <see cref="UnsafeList{T}"/> instance and resizes it accordingly.
    /// </summary>
    /// <param name="min">The minimum amount of items ensured.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int min)
    {
        if (min <= Count) return;

        var requiredBytes = sizeof(T) * min;
        var oldArray = _array;
        var newArray = Marshal.AllocHGlobal(requiredBytes);
        
        // Copy & Free
        Buffer.MemoryCopy(oldArray, (T*)newArray, requiredBytes, Count * sizeof(T));
        Marshal.FreeHGlobal((IntPtr)oldArray);

        _array = (T*)newArray;
        Capacity = min;
    }
    
    /// <summary>
    ///     Trims the capacity of this <see cref="UnsafeList{T}"/> to release unused memory.
    /// </summary>
    /// <param name="min"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        var requiredBytes = sizeof(T) * Count;
        var oldArray = _array;
        var newArray = Marshal.AllocHGlobal(requiredBytes);
        
        // Copy & free
        Buffer.MemoryCopy(oldArray, (T*)newArray, requiredBytes, Count * sizeof(T));
        Marshal.FreeHGlobal((IntPtr)oldArray);

        _array = (T*)newArray;
        Capacity = Count;
    }
    
    /// <summary>
    ///    Acesses an item at the index of the list. 
    /// </summary>
    /// <param name="index">The index.</param>
    T IList<T>.this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array[index];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _array[index] = value;
    }

    /// <summary>
    ///     Acesses an item at the index of the list. 
    /// </summary>
    /// <param name="i">The index.</param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array[i];
    }

    /// <summary>
    ///     Clears this <see cref="UnsafeList{T}"/> instance.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Count = 0;
    }
    
    /// <summary>
    ///     Disposes this instance and releases its memory. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr)_array);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the list content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator<T> GetEnumerator()
    {
        return new UnsafeEnumerator<T>(_array, Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerable{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator<T>(_array, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator<T>(_array, Count);
    }

    /// <summary>
    ///     Converts this <see cref="UnsafeList{T}"/> to a string.
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
        return $"UnsafeList<{typeof(T).Name}>[{Count}]{{{items}}}";
    }
}

/// <summary>
///     The <see cref="Enumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for the <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe struct Enumerator<T> : IEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="Enumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Enumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = 0;
    }

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    public T Current => _list[_index-1];

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    object IEnumerator.Current => _list[_index-1];

    /// <summary>
    ///     Disposes this enumerator.
    /// </summary>
    public void Dispose() { }   // nop

    /// <summary>
    ///     Moves to the next item.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        return unchecked(_index++ < _count);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = 0;
    }
}

/// <summary>
///     The <see cref="Enumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for the <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe struct UnsafeEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="Enumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal UnsafeEnumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = 0;
    }

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    public ref T Current => ref _list[_index-1];

    /// <summary>
    ///     Moves to the next item.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        return unchecked(_index++ < _count);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = 0;
    }
}

/// <summary>
///     A debug view for the <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T">The unmanaged type.</typeparam>
internal class UnsafeListDebugView<T> where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly UnsafeList<T> _entity;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
        get
        {
            var items = new T[_entity.Count];
            _entity.CopyTo(items, 0);
            return items;
        }
    }

    public UnsafeListDebugView(UnsafeList<T> entity) => _entity = entity;
}