using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    ///     The count of this <see cref="UnsafeArray{T}"/> instance, its capacity.
    /// </summary>
    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     The count of this <see cref="UnsafeArray{T}"/> instance, its capacity.
    /// </summary>
    public readonly int Length
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
}

public unsafe struct UnsafeArray
{
    /// <summary>
    ///  Copies the a part of the <see cref="UnsafeArray{T}"/> to the another <see cref="UnsafeArray{T}"/>.
    /// </summary>
    /// <param name="source">The source <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="index">The start index in the source <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="destination">The destination <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="destinationIndex">The start index in the destination <see cref="UnsafeArray{T}"/>.</param>
    /// <param name="length">The length indicating the amount of items being copied.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    internal static void Copy<T>(ref UnsafeArray<T> source, int index, ref UnsafeArray<T> destination, int destinationIndex, int length) where T : unmanaged
    {
        var size = sizeof(T);
        var bytes = size * length;
        var sourcePtr = (void*)(source._ptr + (size*index));
        var destinationPtr = (void*)(destination._ptr + (size*destinationIndex));
        Buffer.MemoryCopy(sourcePtr, destinationPtr, bytes, bytes);
    }


    /// <summary>
    ///     Fills an <see cref="UnsafeArray{T}"/> with a given value.
    /// </summary>
    /// <param name="source">The <see cref="UnsafeArray{T}"/> instance.</param>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    internal static void Fill<T>(ref UnsafeArray<T> source, in T value = default) where T : unmanaged
    {
        for (int index = 0; index < source.Count; index++)
        {
            source[index] = value;
        }
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