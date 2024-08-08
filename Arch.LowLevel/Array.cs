using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace Arch.LowLevel;


/// <summary>
///     The <see cref="Array{T}"/> struct
///     represents an allocated array of managed items which wraps them to acess the items via an unsafe operations.
/// </summary>
/// <typeparam name="T">The managed generic.</typeparam>
[DebuggerTypeProxy(typeof(ArrayDebugView<>))]
public readonly struct Array<T> 
{
    
    /// <summary>
    ///     The static empty <see cref="Array{T}"/>.
    /// </summary>
    internal static Array<T> Empty = new(0);
    
    /// <summary>
    ///     The pointer, pointing towards the first element of this <see cref="Array{T}"/>.
    /// </summary>
    internal readonly T[] _array;

    /// <summary>
    ///     Creates an instance of the <see cref="Array{T}"/>.
    ///     Allocates the array for the passed count of items.
    /// </summary>
    /// <param name="count">The arrays count or capacity.</param>
    public Array(int count)
    {
        _array = new T[count];
        Count = count;
    }

    /// <summary>
    ///     Creates an instance of the <see cref="Array{T}"/>.
    ///     Allocates the array for the passed count of items.
    /// </summary>
    /// <param name="array">The array used.</param>
    public Array(T[] array)
    {
        this._array = array;
        Count = array.Length;
    }

    /// <summary>
    ///     The count of this <see cref="Array{T}"/> instance, its capacity.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     The count of this <see cref="UnsafeArray{T}"/> instance, its capacity.
    /// </summary>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Count;
    }

    /// <summary>
    ///     Returns a reference to an item at a given index.
    /// </summary>
    /// <param name="i">The index.</param>
    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array.DangerousGetReferenceAt(i);
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeArray{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref this[0], Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the array content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator<T> GetEnumerator()
    {
        return new Enumerator<T>(AsSpan());
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="other">The other <see cref="UnsafeArray"/>.</param>
    /// <returns>True if equal, oterwhise false.</returns>
    public bool Equals(Array<T> other)
    {
        return _array == other._array && Count == other.Count;
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="obj">The other <see cref="UnsafeArray"/>.</param>
    /// <returns>True if equal, oterwhise false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Array<T> other && Equals(other);
    }
    
    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeArray"/>.</param>
    /// <param name="right">The second <see cref="UnsafeArray"/>.</param>
    /// <returns></returns>
    public static bool operator ==(Array<T> left, Array<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeArray"/>.</param>
    /// <param name="right">The second <see cref="UnsafeArray"/>.</param>
    /// <returns></returns>
    public static bool operator !=(Array<T> left, Array<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Returns the hash of this <see cref="UnsafeArray"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return _array.GetHashCode();
    }
    
    /// <summary>
    ///     Converts an <see cref="Array{T}"/> into a generic array.
    /// </summary>
    /// <param name="instance">The <see cref="Array{T}"/> instance.</param>
    /// <returns>The array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T[](Array<T> instance)
    {
        return instance._array;
    }
    
    /// <summary>
    ///     Converts an <see cref="Array{T}"/> into a generic array.
    /// </summary>
    /// <param name="instance">The <see cref="Array{T}"/> instance.</param>
    /// <returns>The array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Array<T>(T[] instance)
    {
        return new Array<T>(instance);
    }
    
    /// <summary>
    ///     Converts this <see cref="UnsafeArray{T}"/> to a string.
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
        return $"Array<{typeof(T).Name}>[{Count}]{{{items}}}";
    }
}


public unsafe struct Array
{

    /// <summary>
    ///     Returns an empty <see cref="UnsafeArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <returns>The empty <see cref="UnsafeArray{T}"/>.</returns>
    public static Array<T> Empty<T>()
    {
        return Array<T>.Empty;
    }
    
    /// <summary>
    ///  Copies the a part of the <see cref="UnsafeArray{T}"/> to the another <see cref="UnsafeArray{T}"/>.
    /// </summary>
    /// <param name="source">The source <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="index">The start index in the source <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="destination">The destination <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="destinationIndex">The start index in the destination <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="length">The length indicating the amount of items being copied.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(ref Array<T> source, int index, ref Array<T> destination, int destinationIndex, int length)
    {
        System.Array.Copy(source._array, index, destination._array, destinationIndex, length);
    }


    /// <summary>
    ///     Fills an <see cref="UnsafeArray{T}"/> with a given value.
    /// </summary>
    /// <param name="source">The <see cref="UnsafeArray{T}"/> instance.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(ref Array<T> source, in T value = default) 
    {
        source.AsSpan().Fill(value);
    }
    
    /// <summary>
    ///     Resizes an <see cref="UnsafeArray{T}"/> to a new <see cref="newCapacity"/>.
    /// </summary>
    /// <param name="source">The <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="newCapacity">The new capacity.</param>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <returns>The new resized <see cref="UnsafeArray{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Array<T> Resize<T>(ref Array<T> source, int newCapacity) 
    {
        // Create a new array with the new capacity
        var destination = new Array<T>(newCapacity);
    
        // Calculate the number of elements to copy
        var lengthToCopy = Math.Min(source.Length, newCapacity);
        Copy(ref source, 0, ref destination, 0, lengthToCopy);
        return destination;
    }
}

/// <summary>
///     A debug view for the <see cref="UnsafeArray{T}"/>.
/// </summary>
/// <typeparam name="T">The unmanaged type.</typeparam>
internal class ArrayDebugView<T> where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Array<T> _entity;

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

    public ArrayDebugView(Array<T> entity) => _entity = entity;
}