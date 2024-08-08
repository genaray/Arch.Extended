using System.Collections;
using System.Runtime.CompilerServices;

namespace Arch.LowLevel;

/// <summary>
///     The <see cref="Enumerator{T}"/> is a basic implementation of an enumerator for the <see cref="Array{T}"/>>.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe ref struct Enumerator<T> 
{
    private readonly Span<T> _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="Enumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="Span{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Enumerator(Span<T> list)
    {
        _list = list;
        _count = list.Length;
        _index = -1;
    }

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    public readonly ref T Current => ref _list[_index];

    /// <summary>
    ///     Moves to the next item.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        return unchecked(++_index < _count);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = -1;
    }
}

/// <summary>
///     The <see cref="UnsafeIEnumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for the <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe struct UnsafeIEnumerator<T> : IEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeIEnumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal UnsafeIEnumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = -1;
    }

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    public T Current => _list[_index];

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    object IEnumerator.Current => _list[_index];

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
        return unchecked(++_index < _count);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = -1;
    }
}

/// <summary>
///     The <see cref="UnsafeIEnumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for the <see cref="UnsafeList{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe ref struct UnsafeEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeIEnumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal UnsafeEnumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = -1;
    }

    /// <summary>
    ///     Returns the current item.
    /// </summary>
    public ref T Current => ref _list[_index];

    /// <summary>
    ///     Moves to the next item.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        return unchecked(++_index < _count);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = -1;
    }
}

/// <summary>
///     The <see cref="ReverseIEnumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for iterating backwards.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe struct ReverseIEnumerator<T> : IEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeIEnumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReverseIEnumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = count+1;
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
        return unchecked(--_index > 0);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = _count+1;
    }
}
/// <summary>
///     The <see cref="UnsafeReverseEnumerator{T}"/> is a basic implementation of the <see cref="IEnumerator{T}"/> interface for iterating backwards.
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe ref struct UnsafeReverseEnumerator<T> where T : unmanaged
{
    private readonly T* _list;
    private readonly int _count;
    private int _index;

    /// <summary>
    ///     Creates an instance of the <see cref="UnsafeIEnumerator{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="UnsafeList{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal UnsafeReverseEnumerator(T* list, int count)
    {
        _list = list;
        _count = count;
        _index = count+1;
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
        return unchecked(--_index > 0);
    }

    /// <summary>
    ///     Resets the enumerator.
    /// </summary>
    public void Reset()
    {
        _index = _count+1;
    }
}