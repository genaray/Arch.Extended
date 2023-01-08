using System.Runtime.CompilerServices;

namespace Arch.System;

/// <summary>
///     An interface providing several methods for a system. 
/// </summary>
/// <typeparam name="T">The type passed to each method. For example a delta time or some other data.</typeparam>
public interface ISystem<T> : IDisposable
{
    
    /// <summary>
    ///     Initializes a system, before its first ever run.
    /// </summary>
    void Initialize();
    
    /// <summary>
    ///     Runs before <see cref="Update"/>.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void BeforeUpdate(in T t);
    
    /// <summary>
    ///     Updates the system.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void Update(in T t);
    
    /// <summary>
    ///     Runs after <see cref="Update"/>.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void AfterUpdate(in T t);
}

/// <summary>
///     A basic implementation of a <see cref="ISystem{T}"/>.
/// </summary>
/// <typeparam name="W">The world type.</typeparam>
/// <typeparam name="T">The type passed to the <see cref="ISystem{T}"/> interface.</typeparam>
public abstract class BaseSystem<W, T> : ISystem<T>
{

    private T _data;
    
    /// <summary>
    ///     Creates an instance. 
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    protected BaseSystem(W world)
    {
        World = world;
    }
 
    /// <summary>
    ///     The world instance. 
    /// </summary>
    public W World { get; private set; }

    /// <summary>
    ///     The systems data.
    ///     Assigned during <see cref="BeforeUpdate"/>
    /// </summary>
    public ref T Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _data;
    }

    public virtual void Initialize(){}

    public virtual void BeforeUpdate(in T t)
    {
        _data = t;
    }
    public virtual void Update(in T t){}
    public virtual void AfterUpdate(in T t){}
    public virtual void Dispose(){}
}

/// <summary>
///     A group of <see cref="ISystem{T}"/>'s to organize them.
///     They will run in order.
/// </summary>
/// <typeparam name="T">The type passed to the <see cref="ISystem{T}"/>.</typeparam>
public class Group<T> : ISystem<T>
{

    /// <summary>
    /// All <see cref="ISystem{T}"/>'s in this group. 
    /// </summary>
    private readonly List<ISystem<T>> _systems;

    /// <summary>
    ///     Creates an instance with an array of <see cref="ISystem{T}"/>'s that will belong to this group.
    /// </summary>
    /// <param name="systems">An <see cref="ISystem{T}"/> array.</param>
    public Group(params ISystem<T>[] systems)
    {
        _systems = new List<ISystem<T>>(systems);
    }

    /// <summary>
    ///     Adds several new <see cref="ISystem{T}"/>'s to this group.
    /// </summary>
    /// <param name="systems">An <see cref="ISystem{T}"/> array.</param>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add(params ISystem<T>[] systems)
    {
        _systems.AddRange(systems);
        return this;
    }
    
    /// <summary>
    ///     Adds an single <see cref="ISystem{T}"/> to this group by its generic.
    ///     Automaticly initializes it properly. Must be contructorless.
    /// </summary>
    /// <typeparam name="G">Its generic type.</typeparam>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add<G>() where G : ISystem<T>, new()
    {
        _systems.Add(new G());
        return this;
    }
    
    /// <summary>
    ///     Initializes all <see cref="ISystem{T}"/>'s in this <see cref="Group{T}"/>.
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem{T}.Initialize"/> method.</param>
    public void Initialize()
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.Initialize();
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem{T}.BeforeUpdate"/> on each <see cref="ISystem{T}"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem{T}.Initialize"/> method.</param>
    public void BeforeUpdate(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.BeforeUpdate(in t);
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem{T}.Update"/> on each <see cref="ISystem{T}"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem{T}.Initialize"/> method.</param>
    public void Update(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.Update(in t);
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem{T}.AfterUpdate"/> on each <see cref="ISystem{T}"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem{T}.Initialize"/> method.</param>
    public void AfterUpdate(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.AfterUpdate(in t);
        }
    }

    /// <summary>
    ///     Disposes this <see cref="Group{T}"/> and all <see cref="ISystem{T}"/>'s within.
    /// </summary>
    public void Dispose()
    {
        foreach (var system in _systems)
            system.Dispose();
    }
}