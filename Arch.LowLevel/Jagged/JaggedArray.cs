using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

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
        get => ref Array.DangerousGetReferenceAt(i);
    }
    
    /// <summary>
    ///     Clears this <see cref="Bucket{T}"/> and sets all values to the <see cref="filler"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(T filler = default)
    {
        System.Array.Fill(Array, filler);
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
    ///     The <see cref="_bucketSize"/> is always a value the power of 2, therefore we can use a bitshift for the division during the index calculation. 
    /// </summary>
    private readonly int _bucketSizeShift;
    
    /// <summary>
    ///     The allocated <see cref="Bucket{T}"/>s.
    /// </summary>
    private Array<Bucket<T>> _buckets;

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
        _bucketSizeShift = (int)Math.Log(_bucketSize, 2);
        _buckets = new Array<Bucket<T>>(capacity/_bucketSize + 1);
        
        _filler = default!;

        // Fill buckets
        for (var i = 0; i < _buckets.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            SetBucket(i, in bucket);
            bucket.Clear(_filler);
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
        _bucketSizeShift = (int)Math.Log(_bucketSize, 2);
        _buckets = new Bucket<T>[capacity/_bucketSize + 1];
        
        _filler = filler;

        // Fill buckets
        for (var i = 0; i < _buckets.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            SetBucket(i, in bucket);
            bucket.Clear(_filler);
        }
    }
    
    /// <summary>
    ///     The capacity, the total amount of items. 
    /// </summary>
    public int Capacity => _buckets.Length * _bucketSize;

    /// <summary>
    ///     The length, the buckets inside the <see cref="_buckets"/>.
    /// </summary>
    public int Buckets => _buckets.Length;

    /// <summary>
    ///     Adds an item to the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The item.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int index, in T item)
    {
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        
        ref var bucket = ref GetBucket(bucketIndex);
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
        
        ref var bucket = ref GetBucket(bucketIndex);
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
        if (index < 0 || index >= Capacity)
        {
            value = _filler;
            return false;
        }

        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        ref var item = ref GetBucket(bucketIndex)[itemIndex];

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
        if (index < 0 || index >= Capacity)
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }
        
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        ref var item = ref GetBucket(bucketIndex)[itemIndex];

        // If the item is the default then the nobody set its value.
        if (EqualityComparer<T>.Default.Equals(item, _filler))
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }

        @bool = true;
        return ref item; 
    }
    
    /// <summary>
    ///     Checks if the value at the given index exists.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>True if it does, false if it does not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(int index)
    {
        if (index < 0 || index > Capacity)
        {
            return false;
        }
        
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        ref var item = ref GetBucket(bucketIndex)[itemIndex];

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
        _buckets = Array.Resize(ref _buckets, buckets);

        for (var i = length; i < _buckets.Length; i++)
        {
            var bucket = new Bucket<T>(_bucketSize);
            SetBucket(i, in bucket);
            bucket.Clear(_filler);
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
        for (var i = _buckets.Length-1; i >= 0; i--)
        {
            ref var bucket = ref GetBucket(i);
            if (!bucket.IsEmpty)
            {
                break;
            }

            count++;
        }

        var buckets = _buckets.Length-count;
        _buckets = Array.Resize(ref _buckets, buckets);
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
        bucketIndex = id >> _bucketSizeShift;
        itemIndex = id & _bucketSizeMinusOne;
    }
    
    /// <summary>
    ///     Returns the <see cref="Bucket{T}"/> from the <see cref="_buckets"/> at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="Bucket{T}"/> at the given index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Bucket<T> GetBucket(int index)
    {
        return ref _buckets[index];
    }

    /// <summary>
    ///     Sets the <see cref="Bucket{T}"/> of the <see cref="_buckets"/> at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="bucket">The <see cref="Bucket{T}"/> to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBucket(int index, in Bucket<T> bucket)
    {
        _buckets[index] = bucket;
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
            return ref GetBucket(bucketIndex)[itemIndex];
        }
    }
    
    /// <summary>
    ///     Clears this <see cref="JaggedArray{T}"/> and sets all values to the <see cref="_filler"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        foreach (var bucket in _buckets)
        {
            if (bucket.IsEmpty)
            {
                continue;
            }
            
            bucket.Clear(_filler);
        }
    }
}