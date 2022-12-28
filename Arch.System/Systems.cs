namespace Arch.System;

/// <summary>
/// An interface providing several methods for a system. 
/// </summary>
/// <typeparam name="T">The type passed to each method.</typeparam>
public interface ISystem<T> : IDisposable
{
    void Initialize(in T t);
    void BeforeUpdate(in T t);
    void Update(in T t);
    void AfterUpdate(in T t);
}

/// <summary>
/// A basic implementation of a system.
/// </summary>
/// <typeparam name="W">The world type.</typeparam>
/// <typeparam name="T">The type passed to the <see cref="ISystem{T}"/> interface.</typeparam>
public abstract class BaseSystem<W, T> : ISystem<T>
{
    
    /// <summary>
    /// The world instance. 
    /// </summary>
    public W World { get; private set; }

    protected BaseSystem(W world)
    {
        World = world;
    }

    public virtual void Initialize(in T t){}
    public virtual void BeforeUpdate(in T t){}
    public virtual void Update(in T t){}
    public virtual void AfterUpdate(in T t){}
    public virtual void Dispose(){}
}

/// <summary>
/// A group of <see cref="ISystem{T}"/>'s to organize them.
/// </summary>
/// <typeparam name="T">The type passed to the <see cref="ISystem{T}"/>.</typeparam>
public class Group<T> : ISystem<T>
{

    /// <summary>
    /// All systems in this group. 
    /// </summary>
    private readonly List<ISystem<T>> _systems;

    public Group(params ISystem<T>[] systems)
    {
        _systems = new List<ISystem<T>>(systems);
    }

    public void Initialize(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.Initialize(in t);
        }
    }

    public void BeforeUpdate(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.BeforeUpdate(in t);
        }
    }

    public void Update(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.Update(in t);
        }
    }

    public void AfterUpdate(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.AfterUpdate(in t);
        }
    }

    public void Dispose()
    {
        foreach (var system in _systems)
            system.Dispose();
    }
}

/// <summary>
/// Stores <see cref="ISystem{T}"/>'s to invoke them. 
/// </summary>
/// <typeparam name="T">The type being passed to each of them.</typeparam>
public class Universe<T> : IDisposable
{
    
    /// <summary>
    /// The systems in this universe. 
    /// </summary>
    private List<ISystem<T>> _systems;
    
    public Universe(params ISystem<T>[] systems)
    {
        _systems = new List<ISystem<T>>(systems);
    }

    public Universe<T> Add(params ISystem<T>[] systems)
    {
        _systems.AddRange(systems);
        return this;
    }
    
    public Universe<T> Add<G>() where G : ISystem<T>, new()
    {
        _systems.Add(new G());
        return this;
    }

    public void Initialize(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.Initialize(in t);
        }
    }
    
    public void Update(in T t)
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var system = _systems[index];
            system.BeforeUpdate(in t);
            system.Update(in t);
            system.AfterUpdate(in t);
        }
    }

    public void Dispose()
    {
        foreach (var system in _systems)
            system.Dispose();
    }
}