using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Arch.LowLevel.Jagged;

/// <summary>
///     The <see cref="MathExtensions"/>
///     contains several methods for math operations.
/// </summary>
internal static class MathExtensions
{
    /// <summary>
    /// This method will round down to the nearest power of 2 number. If the supplied number is a power of 2 it will return it.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RoundToPowerOfTwo(int num)
    {
        // If num is a power of 2, return it
        if (num > 0 && (num & (num - 1)) == 0)
        {
            return num;
        }

        // Find the exponent of the nearest power of 2 (rounded down)
        var exponent = (int)Math.Floor(Math.Log(num) / Math.Log(2));

        // Calculate the nearest power of 2
        var result = (int)Math.Pow(2, exponent);

        return result;
    }
}

/// <summary>
///     The <see cref="Bucket{T}"/> struct
///     represents a bucket of the <see cref="JaggedArray{T}"/> where items are stored
/// </summary>
/// <typeparam name="T"></typeparam>
public record struct Bucket<T>
{
    /// <summary>
    ///     The items array.
    /// </summary>
    internal readonly T[] Array = System.Array.Empty<T>();
    
    /// <summary>
    ///     Creates an instance of the <see cref="Bucket{T}"/>.
    /// </summary>
    /// <param name="capacity">The capacity</param>
    public Bucket(int capacity)
    {
        Array = new T[capacity];
    }
    
    /// <summary>
    ///     The amount of items in this <see cref="Bucket{T}"/>.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set;
    }

    /// <summary>
    ///     If this <see cref="Bucket{T}"/> is empty.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Count <= 0;
    }
    
    /// <summary>
    ///     Returns a reference to an item at the given index.
    /// </summary>
    /// <param name="i">The index.</param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Array[i];
    }
}

/// <summary>
///     The <see cref="JaggedArray{T}"/> class,
///     represents a jagged array with <see cref="Bucket{T}"/>s storing the items.
/// </summary>
/// <typeparam name="T"></typeparam>
public class JaggedArray<T>
{
    /// <summary>
    ///     The <see cref="Bucket{T}"/> size in items.
    /// </summary>
    private readonly int _bucketSize;
    
    /// <summary>
    ///     The <see cref="Bucket{T}"/> size in items - 1.
    /// </summary>
    private readonly int _bucketSizeMinusOne;

    /// <summary>
    ///     The allocated <see cref="Bucket{T}"/>s.
    /// </summary>
    private Bucket<T>[] _bucketArray;

    /// <summary>
    ///     The filler, the default value.
    /// </summary>
    private readonly T _filler;
    
    /// <summary>
    ///     Creates an instance of the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="bucketSize">The <see cref="Bucket{T}"/> size in bytes.</param>
    /// <param name="capacity">The total initial capacity, how many items should fit in.</param>
    public JaggedArray(int bucketSize, int capacity = 64)
    {
        _bucketSize = MathExtensions.RoundToPowerOfTwo(bucketSize);
        _bucketSizeMinusOne = _bucketSize - 1;
        _bucketArray = new Bucket<T>[capacity/_bucketSize + 1];
        
        _filler = default!;

        // Fill buckets
        for (var i = 0; i < _bucketArray.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            _bucketArray[i] = bucket;
            Array.Fill(bucket.Array, _filler);
        }
    }

    /// <summary>
    ///     Creates an instance of the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="bucketSize">The <see cref="Bucket{T}"/> size in bytes.</param>
    /// <param name="filler">The filler value for all slots, basically a custom default-value.</param>
    /// <param name="capacity">The total initial capacity, how many items should fit in.</param>
    public JaggedArray(int bucketSize, T filler, int capacity = 64) : this(bucketSize, capacity)
    {
        _bucketSize = MathExtensions.RoundToPowerOfTwo(bucketSize);
        _bucketSizeMinusOne = _bucketSize - 1;
        _bucketArray = new Bucket<T>[capacity/_bucketSize + 1];
        
        _filler = filler;

        // Fill buckets
        for (var i = 0; i < _bucketArray.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            _bucketArray[i] = bucket;
            Array.Fill(bucket.Array, _filler);
        }
    }
    
    /// <summary>
    ///     The capacity, the total amount of items. 
    /// </summary>
    public int Capacity => _bucketArray.Length * _bucketSize;

    /// <summary>
    ///     The length, the buckets inside the <see cref="_bucketArray"/>.
    /// </summary>
    public int Buckets => _bucketArray.Length;

    /// <summary>
    ///     Adds an item to the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int index, in T item)
    {
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        
        ref var bucket = ref _bucketArray[bucketIndex];
        bucket[itemIndex] = item;
        bucket.Count++;
    }

    /// <summary>
    ///     Removes an item from the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="index">The index.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int index)
    {
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        
        ref var bucket = ref _bucketArray[bucketIndex];
        bucket[itemIndex] = _filler;
        bucket.Count--;
    }
    
    /// <summary>
    ///     Trys to get an item from its index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The returned value.</param>
    /// <returns>True if sucessfull, otherwhise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(int index, out T value)
    {
        // If the id is negative
        if (index < 0)
        {
            value = _filler;
            return false;
        }
        
        if (index >= Capacity)
        {
            value = _filler;
            return false;
        }

        IndexToSlot(index, out var bucketIndex, out var itemIndex);

        // If the item is outside the array. Then it definetly doesn't exist
        if (bucketIndex > _bucketArray.Length)
        {
            value = _filler;
            return false;
        }

        ref var item = ref _bucketArray[bucketIndex][itemIndex];

        // If the item is the default then the nobody set its value.
        if (EqualityComparer<T>.Default.Equals(item, _filler))
        {
            value = _filler;
            return false;
        }

        value = item;
        return true;
    }
    
    /// <summary>
    ///     Trys to get an item from its index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="bool">True if sucessfull, otherwhise false</param>
    /// <returns>A reference or null reference to the item.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGetValue(int index, out bool @bool)
    {
        // If the id is negative
        if (index < 0)
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }

        IndexToSlot(index, out var bucketIndex, out var itemIndex);

        // If the item is outside the array. Then it definetly doesn't exist
        if (bucketIndex > _bucketArray.Length)
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }

        ref var item = ref _bucketArray[bucketIndex][itemIndex];

        // If the item is the default then the nobody set its value.
        if (EqualityComparer<T>.Default.Equals(item, _filler))
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }

        @bool = true;
        return ref Unsafe.NullRef<T>(); 
    }
    
    /// <summary>
    ///     Checks if the value at the given index exists.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>True if it does, false if it does not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(int index)
    {
        if (index <= 0 || index > Capacity)
        {
            return false;
        }
        
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        ref var item = ref _bucketArray[bucketIndex][itemIndex];

        // If the item is the default then the nobody set its value.
        return !EqualityComparer<T>.Default.Equals(item, _filler);
    }

    /// <summary>
    ///     Ensures the capacity and increases it if necessary.
    /// </summary>
    /// <param name="newCapacity">The new capcity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int newCapacity)
    {
        if (newCapacity < Capacity)
        {
            return;
        }

        var length = Buckets;
        var buckets = newCapacity / _bucketSize + 1;
        Array.Resize(ref _bucketArray, buckets);

        for (var i = length; i < _bucketArray.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            _bucketArray[i] = bucket;
            Array.Fill(bucket.Array, _filler);
        }
    }

    /// <summary>
    ///     Trims the last few empty buckets to release memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        // Count how many of the last buckets are empty, to trim them
        var count = 0;
        for (var i = _bucketArray.Length-1; i >= 0; i--)
        {
            ref var bucket = ref _bucketArray[i];
            if (!bucket.IsEmpty)
            {
                break;
            }

            count++;
        }

        var buckets = _bucketArray.Length-count;
        Array.Resize(ref _bucketArray, buckets);
    }

    /// <summary>
    ///     Converts the passed id to its inner and outer index ( or slot ) inside the <see cref="_items"/> array.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="bucketIndex">The outer index.</param>
    /// <param name="itemIndex">The inner index.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IndexToSlot(int id, out int bucketIndex, out int itemIndex)
    {
        Debug.Assert(id >= 0, "Id cannot be negative.");

        /* Instead of the '%' operator we can use logical '&' operator which is faster. But it requires the bucket size to be a power of 2. */
        bucketIndex = id / _bucketSize;
        itemIndex = id & _bucketSizeMinusOne;
    }
    
    /// <summary>
    ///     Returns the <see cref="Bucket{T}"/> from the <see cref="_bucketArray"/> at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="Bucket{T}"/> at the given index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Bucket<T> GetBucket(int index)
    {
        return ref _bucketArray[index];
    }
    
    /// <summary>
    ///     Returns a reference to an item at the given index.
    /// </summary>
    /// <param name="i">The index.</param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            IndexToSlot(i, out var bucketIndex, out var itemIndex);
            return ref _bucketArray[bucketIndex][itemIndex];
        }
    }
    
    /// <summary>
    ///     Clears this <see cref="JaggedArray{T}"/> and sets all values to the <see cref="_filler"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        foreach (var bucket in _bucketArray)
        {
            if (bucket.IsEmpty)
            {
                continue;
            }
            
            Array.Fill(bucket.Array, _filler);
        }
    }
}