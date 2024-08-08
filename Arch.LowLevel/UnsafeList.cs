using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    private UnsafeArray<T> _array;
    
    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeList{T}"/>.
    /// </summary>
    /// <param name="capacity">The initial capacity that is being allocated.</param>
    public UnsafeList(int capacity = 8)
    {
        Count = 0;
        Capacity = capacity;
        _array = new UnsafeArray<T>(capacity);
    }

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeList{T}"/> by a pointer.
    /// </summary>
    /// <param name="ptr">The pointer.</param>
    /// <param name="capacity">The initial capacity that is being allocated.</param>
    public UnsafeList(T* ptr, int capacity = 8)
    {
        Count = 0;
        Capacity = capacity;
        _array = new UnsafeArray<T>(ptr, capacity);
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
        if (Count == Capacity)
        {
            EnsureCapacity(Capacity * 2);
        }

        _array[Count] = item;
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
            throw new ArgumentOutOfRangeException(nameof(index)); 
        }
        
        // Resize if the list is actually full
        if (Capacity == Count) 
        {
            EnsureCapacity(Capacity + 1);
        }

        if (index < Count)
        {
            //var span = _array.AsSpan();
            //var src = span.Slice(index, Count - index);
            //var dst = span.Slice(index + 1, src.Length);
            //src.CopyTo(dst);

            UnsafeArray.Copy(ref _array, index, ref _array, index + 1, Count - index);
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
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        Count--;
        if (index < Count) 
        {
            //Buffer.MemoryCopy(_array+(index+1), _array+index,Count-index,Count-index);
            UnsafeArray.Copy(ref _array, index + 1, ref _array, index, Count - index);
        }
        _array[Count] = default;
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
        if (Count == 0)
            return;
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new IndexOutOfRangeException("Index must be 0 <= index <= array.Length");
        if (arrayIndex + Count > array.Length)
            throw new ArgumentException("Destination array was not long enough. Check the destination index, length, and the array's lower bounds.", nameof(arrayIndex));

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
        if (min <= Count)
        {
            return;
        }
        
        var oldArray = _array;
        var newArray = new UnsafeArray<T>(min);
        
        // Copy & Free
        UnsafeArray.Copy(ref oldArray, 0, ref newArray,0, Count);
        oldArray.Dispose();

        _array = newArray;
        Capacity = min;
    }
    
    /// <summary>
    ///     Trims the capacity of this <see cref="UnsafeList{T}"/> to release unused memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        var oldArray = _array;
        var newArray = new UnsafeArray<T>(Count);
        
        // Copy & free
        UnsafeArray.Copy(ref oldArray, 0, ref newArray,0, Count);
        oldArray.Dispose();
        
        _array = newArray;
        Capacity = Count;
    }
    
    /// <summary>
    ///    Acesses an item at the index of the list. 
    /// </summary>
    /// <param name="index">The index.</param>
    T IList<T>.this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array[CheckIndex(index)];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _array[CheckIndex(index)] = value;
    }

    /// <summary>
    ///     Acesses an item at the index of the list. 
    /// </summary>
    /// <param name="i">The index.</param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array[CheckIndex(i)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int CheckIndex(int index)
    {
#if DEBUG
        if (index < 0)
            throw new IndexOutOfRangeException("Index cannot be less than zero");
        if (index >= Count)
            throw new IndexOutOfRangeException("Index cannot be greater than or equal to the count");
#endif

        return index;
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
        _array.Dispose();
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeList{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(_array, Count);
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
        return new UnsafeIEnumerator<T>(_array, Count);
    }

    /// <summary>
    ///     Creates an instance of a <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>The new <see cref="IEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new UnsafeIEnumerator<T>(_array, Count);
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="other">The other <see cref="UnsafeList{T}"/>.</param>
    /// <returns>True or false.</returns>
    public bool Equals(UnsafeList<T> other)
    {
        return _array.Equals(other._array) && Count == other.Count && Capacity == other.Capacity;
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="obj">The other <see cref="UnsafeList{T}"/>.</param>
    /// <returns>True or false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is UnsafeList<T> other && Equals(other);
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeList{T}"/>.</param>
    /// <param name="right">The second <see cref="UnsafeList{T}"/>.</param>
    /// <returns>True or false.</returns>
    public static bool operator ==(UnsafeList<T> left, UnsafeList<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeList{T}"/>.</param>
    /// <param name="right">The second <see cref="UnsafeList{T}"/>.</param>
    /// <returns>True or false.</returns>
    public static bool operator !=(UnsafeList<T> left, UnsafeList<T> right)
    {
        return !left.Equals(right);
    }
    
    /// <summary>
    ///     Returns the hashcode of this <see cref="UnsafeList{T}"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _array.GetHashCode();
            hashCode = (hashCode * 397) ^ Count;
            hashCode = (hashCode * 397) ^ Capacity;
            return hashCode;
        }
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