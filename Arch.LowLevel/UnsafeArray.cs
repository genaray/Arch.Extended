using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Arch.LowLevel;

/// <summary>
///     The <see cref="UnsafeArray{T}"/> struct
///     represents an unsafe allocated array of unmanaged items.
/// </summary>
/// <typeparam name="T">The unmanaged generic.</typeparam>
[DebuggerTypeProxy(typeof(UnsafeArrayDebugView<>))]
public readonly unsafe struct UnsafeArray<T> : IDisposable where T : unmanaged
{

    /// <summary>
    ///     The static empty <see cref="UnsafeArray{T}"/>.
    /// </summary>
    internal static UnsafeArray<T> Empty = new(null, 0);
    
    /// <summary>
    ///     The pointer, pointing towards the first element of this <see cref="UnsafeArray{T}"/>.
    /// </summary>
    internal readonly T* _ptr;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeArray{T}"/>.
    ///     Allocates the array for the passed count of items.
    /// </summary>
    /// <param name="count">The arrays count or capacity.</param>
    public UnsafeArray(int count)
    {
#if NET6_0_OR_GREATER
        _ptr = (T*)NativeMemory.Alloc((nuint)(sizeof(T) * count));
#else
        _ptr = (T*)Marshal.AllocHGlobal(sizeof(T) * count);
#endif
        Count = count;
    }
    
    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeArray{T}"/> by a pointer.
    /// </summary>
    /// <param name="ptr">The pointer.</param>
    /// <param name="count">The count.</param>
    public UnsafeArray(T* ptr, int count)
    {
        _ptr = ptr;
        Count = count;
    }

    /// <summary>
    ///     The count of this <see cref="UnsafeArray{T}"/> instance, its capacity.
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
        get => ref _ptr[i];
    }

    /// <summary>
    ///     Disposes this instance of <see cref="UnsafeArray{T}"/> and releases its memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
#if NET6_0_OR_GREATER
        NativeMemory.Free(_ptr);
#else
        Marshal.FreeHGlobal((IntPtr)_ptr);
#endif
    }

    /// <summary>
    ///     Converts this <see cref="UnsafeArray{T}"/> instance into a <see cref="Span{T}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Span{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return new Span<T>(_ptr, Count);
    }
    
    /// <summary>
    ///     Creates an instance of a <see cref="UnsafeEnumerator{T}"/> for ref acessing the array content.
    /// </summary>
    /// <returns>A new <see cref="UnsafeEnumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator<T> GetEnumerator()
    {
        return new UnsafeEnumerator<T>(_ptr, Count);
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="other">The other <see cref="UnsafeArray"/>.</param>
    /// <returns>True if equal, oterwhise false.</returns>
    public bool Equals(UnsafeArray<T> other)
    {
        return _ptr == other._ptr && Count == other.Count;
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="obj">The other <see cref="UnsafeArray"/>.</param>
    /// <returns>True if equal, oterwhise false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is UnsafeArray<T> other && Equals(other);
    }

    
    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeArray"/>.</param>
    /// <param name="right">The second <see cref="UnsafeArray"/>.</param>
    /// <returns></returns>
    public static bool operator ==(UnsafeArray<T> left, UnsafeArray<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left">The first <see cref="UnsafeArray"/>.</param>
    /// <param name="right">The second <see cref="UnsafeArray"/>.</param>
    /// <returns></returns>
    public static bool operator !=(UnsafeArray<T> left, UnsafeArray<T> right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Returns the hash of this <see cref="UnsafeArray"/>.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return (unchecked((int)(long)_ptr) * 397) ^ Count;
        }
    }
    
    /// <summary>
    ///     Converts an <see cref="UnsafeArray{T}"/> into a void pointer.
    /// </summary>
    /// <param name="instance">The <see cref="UnsafeArray{T}"/> instance.</param>
    /// <returns>A void pointer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void*(UnsafeArray<T> instance)
    {
        return (void*)instance._ptr;
    }
    
    /// <summary>
    ///     Converts an <see cref="UnsafeArray{T}"/> into a generic pointer.
    /// </summary>
    /// <param name="instance">The <see cref="UnsafeArray{T}"/> instance.</param>
    /// <returns>A void pointer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(UnsafeArray<T> instance)
    {
        return instance._ptr;
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
        return $"UnsafeArray<{typeof(T).Name}>[{Count}]{{{items}}}";
    }
}

public unsafe struct UnsafeArray
{

    /// <summary>
    ///     Returns an empty <see cref="UnsafeArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <returns>The empty <see cref="UnsafeArray{T}"/>.</returns>
    public static UnsafeArray<T> Empty<T>() where T : unmanaged
    {
        return UnsafeArray<T>.Empty;
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
    public static void Copy<T>(ref UnsafeArray<T> source, int index, ref UnsafeArray<T> destination, int destinationIndex, int length) where T : unmanaged
    {
        var size = sizeof(T);
        var bytes = size * length;
        var sourcePtr = (void*)(source._ptr + index);
        var destinationPtr = (void*)(destination._ptr + destinationIndex);
        Buffer.MemoryCopy(sourcePtr, destinationPtr, bytes, bytes);
    }


    /// <summary>
    ///     Fills an <see cref="UnsafeArray{T}"/> with a given value.
    /// </summary>
    /// <param name="source">The <see cref="UnsafeArray{T}"/> instance.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill<T>(ref UnsafeArray<T> source, in T value = default) where T : unmanaged
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
    public static UnsafeArray<T> Resize<T>(ref UnsafeArray<T> source, int newCapacity) where T : unmanaged
    {
        var destination = new UnsafeArray<T>(newCapacity);
        Copy(ref source, 0, ref destination, 0, Math.Min(source.Length, destination.Length));

        source.Dispose();
        return destination;
    }
}

/// <summary>
///     A debug view for the <see cref="UnsafeArray{T}"/>.
/// </summary>
/// <typeparam name="T">The unmanaged type.</typeparam>
internal class UnsafeArrayDebugView<T> where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly UnsafeArray<T> _entity;

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

    public UnsafeArrayDebugView(UnsafeArray<T> entity) => _entity = entity;
}