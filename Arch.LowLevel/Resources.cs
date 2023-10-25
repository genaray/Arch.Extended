using System.Runtime.CompilerServices;
using Arch.LowLevel.Jagged;

[assembly: InternalsVisibleTo("Arch.LowLevel.Tests")]
namespace Arch.LowLevel;

/// <summary>
///     The <see cref="Handle{T}"/> struct
///     represents a reference to an managed resource.
///     This is used commonly for referencing managed resources from components.
/// </summary>
/// <typeparam name="T">The type of the managed resource.</typeparam>
public readonly record struct Handle<T>
{
    
    /// <summary>
    ///     A null <see cref="Handle{T}"/> which is invalid and used for camparison.
    /// </summary>
    public static readonly Handle<T> NULL = new(-1);
    
    /// <summary>
    ///     The id, its index inside a <see cref="Resources{T}"/> array.
    /// </summary>
    public readonly int Id = -1;

    /// <summary>
    ///     Public default constructor.
    /// </summary>
    public Handle()
    {
        Id = -1;
    }

    /// <summary>
    ///      Initializes a new instance of the <see cref="Handle{T}" /> class.
    /// </summary>
    /// <param name="id"></param>
    internal Handle(int id)
    {
        Id = id;
    }
}

/// <summary>
///     The <see cref="Handle{T}"/> class,
///     represents an collection of managed resources which can be accesed by a <see cref="Handle{T}"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Resources<T> : IDisposable
{

    /// <summary>
    ///     The <see cref="JaggedArray{T}"/> which stores the managed resources on the <see cref="Handle{T}"/> index.
    /// </summary>
    private JaggedArray<T> _array;

    /// <summary>
    ///     A list of recycled <see cref="Handle{T}"/> ids, used to fill in old gaps.
    /// </summary>
    internal Queue<int> _ids;

    /// <summary>
    ///     Creates an <see cref="Resources{T}"/> instance.
    /// </summary>
    /// <param name="capacity">The capacity of the bucket.</param>
    public Resources(int capacity = 64)
    {
        _array = new JaggedArray<T>(capacity, capacity);
        _ids = new Queue<int>(capacity);
    }

    /// <summary>
    ///     Creates an <see cref="Resources{T}"/> instance.
    /// </summary>
    /// <param name="size">The size of the generic type in bytes.</param>
    /// <param name="capacity">The capcity, how many items of that type should fit into the array.</param>
    public Resources(int size, int capacity = 64)
    {
        _array = new JaggedArray<T>(160000/size, capacity);
        _ids = new Queue<int>(capacity);
    }

    /// <summary>
    ///     The amount of registered <see cref="Handle{T}"/>s.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    /// <summary>
    ///     Creates a <see cref="Handle{T}"/> for the given resource.
    /// </summary>
    /// <param name="item">The resource instance.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> Add(in T item)
    {
        // Create handle
        var recyled = _ids.TryDequeue(out var id);
        id = recyled ? id : Count;
        var handle = new Handle<T>(id);

        // Resize array and fill it in
        _array.EnsureCapacity(id+1);
        _array.Add(id, item);

        Count++;
        return handle;
    }

    /// <summary>
    ///     Checks if the <see cref="Handle{T}"/> is valid.
    /// </summary>
    /// <param name="handle">The <see cref="Handle{T}"/>.</param>
    /// <returns>True or false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(in Handle<T> handle)
    {
        return handle.Id > -1 && handle.Id <= _array.Capacity;
    }
    
    /// <summary>
    ///     Returns a resource for the given <see cref="Handle{T}"/>.
    /// </summary>
    /// <param name="handle">The <see cref="Handle{T}"/>.</param>
    /// <returns>The resource.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(in Handle<T> handle)
    {
        return ref _array[handle.Id];
    }

    /// <summary>
    ///     Removes a <see cref="Handle{T}"/> and its resource.
    /// </summary>
    /// <param name="handle">The <see cref="Handle{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(in Handle<T> handle)
    {
        _array.Remove(handle.Id);
        _ids.Enqueue(handle.Id);

        Count--;
    }

    /// <summary>
    ///     Trims the resources and releases unused memory if possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess()
    {
        _array.TrimExcess();
        _ids.TrimExcess();
    }

    /// <summary>
    ///     Disposes this <see cref="Resources{T}"/> instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _array = null;
        _ids = null;
        Count = 0;
    }
}
