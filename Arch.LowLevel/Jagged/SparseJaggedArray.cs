using CommunityToolkit.HighPerformance;

namespace Arch.LowLevel.Jagged;

using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
///     The <see cref="SparseBucket{T}"/> struct
///     represents a bucket of the <see cref="SparseJaggedArray{T}"/> where items are stored.
///     <remarks>It will not allocate memory upon creation, it stays empty till the first item was added in.</remarks>
/// </summary>
/// <typeparam name="T"></typeparam>
public record struct SparseBucket<T>
{
    /// <summary>
    ///     The items array.
    /// </summary>
    internal T[] Array = System.Array.Empty<T>();
    
    /// <summary>
    ///     The filler, the default value.
    /// </summary>
    private readonly T _filler;

    /// <summary>
    ///     Creates an instance of the <see cref="Bucket{T}"/>.
    /// </summary>
    /// <param name="capacity">The total capacity.</param>
    /// <param name="filler">The filler.</param>
    /// <param name="allocate">If it should allocate straight forward.</param>
    public SparseBucket(int capacity, T filler, bool allocate = false)
    {
        Capacity = capacity;
        _filler = filler;
        if (allocate)
        {
            EnsureCapacity();
        }
    }

    /// <summary>
    ///     The total capacity of this <see cref="Bucket{T}"/>.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
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
    ///     Ensures the <see cref="Capacity"/> of this <see cref="Bucket{T}"/>.
    ///     Basically allocated a new array. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureCapacity()
    {
        if (Array != System.Array.Empty<T>())
        {
            return;
        }
        
        Array = new T[Capacity];
        Clear();
    }

    /// <summary>
    ///     Trims the bucket to an empty one. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void TrimExcess()
    {
        if (Count > 0)
        {
            return;
        }
        
        Array = System.Array.Empty<T>();
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
    ///     Clears this <see cref="SparseBucket{T}"/> and sets all values to the <see cref="_filler"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        System.Array.Fill(Array, _filler);
    }
}

/// <summary>
///     The <see cref="SparseJaggedArray{T}"/> class,
///     represents a jagged array with <see cref="SparseBucket{T}"/>s storing the items.
///     <remarks>Its buckets will stay empty and not allocate memory till a slot in it is being used.</remarks>
/// </summary>
/// <typeparam name="T"></typeparam>
public class SparseJaggedArray<T>
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
    private Array<SparseBucket<T>> _buckets;

    /// <summary>
    ///     The filler, the default value.
    /// </summary>
    private readonly T _filler;
    
    /// <summary>
    ///     Creates an instance of the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="bucketSize">The <see cref="Bucket{T}"/> size in bytes.</param>
    /// <param name="capacity">The total initial capacity, how many items should fit in.</param>
    public SparseJaggedArray(int bucketSize, int capacity = 64)
    {
        _bucketSize = MathExtensions.RoundToPowerOfTwo(bucketSize);
        _bucketSizeMinusOne = _bucketSize - 1;
        _bucketSizeShift = (int)Math.Log(_bucketSize, 2);
        _buckets = new Array<SparseBucket<T>>(capacity/_bucketSize + 1);
        
        _filler = default!;

        // Fill buckets
        for (var i = 0; i < _buckets.Length; i++)
        {
            var bucket = new SparseBucket<T>(_bucketSize, _filler);
            SetBucket(i, in bucket);
            bucket.Clear();
        }
    }

    /// <summary>
    ///     Creates an instance of the <see cref="JaggedArray{T}"/>.
    /// </summary>
    /// <param name="bucketSize">The <see cref="Bucket{T}"/> size in bytes.</param>
    /// <param name="filler">The filler value for all slots, basically a custom default-value.</param>
    /// <param name="capacity">The total initial capacity, how many items should fit in.</param>
    public SparseJaggedArray(int bucketSize, T filler, int capacity = 64) : this(bucketSize, capacity)
    {
        _bucketSize = MathExtensions.RoundToPowerOfTwo(bucketSize);
        _bucketSizeMinusOne = _bucketSize - 1;
        _bucketSizeShift = (int)Math.Log(_bucketSize, 2);
        _buckets = new Array<SparseBucket<T>>(capacity/_bucketSize + 1);
        
        _filler = filler!;

        // Fill buckets
        for (var i = 0; i < _buckets.Length; i++)
        {
            var bucket = new SparseBucket<T>(_bucketSize, filler);
            SetBucket(i, in bucket);
            bucket.Clear();
        }
    }
    
    /// <summary>
    ///     If true, each bucket will stay empty and will not allocate memory until its actually being used. 
    /// </summary>
    public bool Sparse { get; set; }
    
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
        bucket.EnsureCapacity();
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
        bucket.TrimExcess();
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
        
        // Bucket empty? return false
        ref var bucket = ref GetBucket(bucketIndex);
        if (bucket.IsEmpty)
        {
            value = _filler;
            return false;
        }
        
        // If the item is the default then the nobody set its value.
        ref var item = ref bucket[itemIndex];
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
        
        // Bucket empty? return false
        ref var bucket = ref GetBucket(bucketIndex);
        if (bucket.IsEmpty)
        {
            @bool = false;
            return ref Unsafe.NullRef<T>(); 
        }
        
        // If the item is the default then the nobody set its value.
        ref var item = ref bucket[itemIndex];
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
        if (index < 0 || index >= Capacity)
        {
            return false;
        }
        
        IndexToSlot(index, out var bucketIndex, out var itemIndex);
        
        // If bucket empty return false
        ref var bucket = ref GetBucket(bucketIndex);
        if (bucket.IsEmpty)
        {
            return false;
        }

        // If the item is the default then the nobody set its value.
        ref var item = ref bucket[itemIndex];
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
            var bucket = new SparseBucket<T>(_bucketSize, _filler);
            SetBucket(i, bucket);
            bucket.Clear();
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
    ///     Converts the passed id to its inner and outer index ( or slot ) inside the <see cref="_buckets"/> array.
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
    ///     Returns the <see cref="SparseBucket{T}"/> from the <see cref="_buckets"/> at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="SparseBucket{T}"/> at the given index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref SparseBucket<T> GetBucket(int index)
    {
        return ref _buckets[index];
    }
    
    /// <summary>
    ///     Sets the <see cref="SparseBucket{T}"/> of the <see cref="_buckets"/> at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="bucket">The <see cref="SparseBucket{T}"/> to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBucket(int index, in SparseBucket<T> bucket)
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
    ///     Clears this <see cref="SparseJaggedArray{T}"/> and sets all values to the <see cref="_filler"/>.
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
            
            bucket.Clear();
        }
    }
}