//#if !NET5_0_OR_GREATER
    #define ARCH_METRICS_DISABLED
//#endif

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

#if !ARCH_METRICS_DISABLED
using System.Diagnostics.Metrics;
#endif

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

    /// <inheritdoc />
    public virtual void Initialize(){}

    /// <inheritdoc />
    public virtual void BeforeUpdate(in T t) { }

    /// <inheritdoc />
    public virtual void Update(in T t){}

    /// <inheritdoc />
    public virtual void AfterUpdate(in T t){}

    /// <inheritdoc />
    public virtual void Dispose(){}
}

/// <summary>
///     A group of <see cref="ISystem{T}"/>'s to organize them.
///     They will run in order.
/// </summary>
/// <typeparam name="T">The type passed to the <see cref="ISystem{T}"/>.</typeparam>
public class Group<T> : ISystem<T>
{
#if !ARCH_METRICS_DISABLED
    private readonly Meter _meter;
    private readonly Stopwatch _timer = new();
#endif

    /// <summary>
    /// A unique name to identify this group
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// All <see cref="SystemEntry"/>'s in this group. 
    /// </summary>
    private readonly List<SystemEntry> _systems = new();

    /// <summary>
    ///     Creates an instance with an array of <see cref="ISystem{T}"/>'s that will belong to this group.
    /// </summary>
    /// <param name="name">A unique name to identify this group</param>
    /// <param name="systems">An <see cref="ISystem{T}"/> array.</param>
    public Group(string name, params ISystem<T>[] systems)
        : this(name, (IEnumerable<ISystem<T>>)systems)
    {
    }

    /// <summary>
    ///     Creates an instance with an <see cref="IEnumerable{T}"/> of <see cref="ISystem{T}"/>'s that will belong to this group.
    /// </summary>
    /// <param name="name">A unique name to identify this group</param>
    /// <param name="systems">An <see cref="IEnumerable{T}"/> of <see cref="ISystem{T}"/>.</param>
    public Group(string name, IEnumerable<ISystem<T>> systems)
    {
        Name = name;

#if !ARCH_METRICS_DISABLED
        _meter = new Meter(name);
#endif

#if NET5_0_OR_GREATER
        // If possible expand the list before adding all the systems
        if (systems.TryGetNonEnumeratedCount(out var count))
            _systems.Capacity = count;
#endif

        foreach (var system in systems)
            Add(system);
    }

    /// <summary>
    ///     Adds several new <see cref="ISystem{T}"/>'s to this group.
    /// </summary>
    /// <param name="systems">An <see cref="ISystem{T}"/> array.</param>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add(params ISystem<T>[] systems)
    {
        _systems.Capacity = Math.Max(_systems.Capacity, _systems.Count + systems.Length);

        foreach (var system in systems)
            Add(system);

        return this;
    }
    
    /// <summary>
    ///     Adds an single <see cref="ISystem{T}"/> to this group by its generic.
    ///     Automatically initializes it properly. Must be contructorless.
    /// </summary>
    /// <typeparam name="G">Its generic type.</typeparam>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add<G>() where G : ISystem<T>, new()
    {
        return Add(new G());
    }

    /// <summary>
    ///     Adds an single <see cref="ISystem{T}"/> to this group.
    /// </summary>
    /// <param name="system"></param>
    /// <returns></returns>
    public Group<T> Add(ISystem<T> system)
    {
        _systems.Add(new SystemEntry(system
#if !ARCH_METRICS_DISABLED
            , _meter
#endif
        ));

        return this;
    }

    /// <summary>
    ///     Return the first <see cref="G"/> which was found in the hierachy.
    /// </summary>
    /// <typeparam name="G">The Type.</typeparam>
    /// <returns></returns>
    public G Get<G>() where G : ISystem<T>
    {
        foreach (var item in _systems)
        {
            if (item.System is G sys)
            {
                return sys;
            }

            if (item.System is not Group<T> grp)
            {
                continue;
            }

            return grp.Get<G>();
        }

        return default;
    }
    
    /// <summary>
    ///     Finds all <see cref="ISystem{T}"/>s which can be cast into the given type.
    /// </summary>
    /// <typeparam name="G">The Type.</typeparam>
    /// <returns></returns>
    public IEnumerable<G> Find<G>() where G : ISystem<T>
    {
        foreach (var item in _systems)
        {
            if (item.System is G sys)
            {
                yield return sys;
            }

            if (item.System is not Group<T> grp)
            {
                continue;
            }
            
            foreach (var nested in grp.Find<G>())
            {
                yield return nested;   
            }
        }
    }

    /// <summary>
    ///     Initializes all <see cref="ISystem{T}"/>'s in this <see cref="Group{T}"/>.
    /// </summary>
    public void Initialize()
    {
        for (var index = 0; index < _systems.Count; index++)
        {
            var entry = _systems[index];
            entry.System.Initialize();
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
            var entry = _systems[index];

#if !ARCH_METRICS_DISABLED
            _timer.Restart();
            {
#endif

                entry.System.BeforeUpdate(in t);

#if !ARCH_METRICS_DISABLED
            }
            _timer.Stop();
            entry.BeforeUpdate.Record(_timer.Elapsed.TotalMilliseconds);
#endif
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
            var entry = _systems[index];

#if !ARCH_METRICS_DISABLED
            _timer.Restart();
            {
#endif

                entry.System.Update(in t);

#if !ARCH_METRICS_DISABLED
            }
            _timer.Stop();
            entry.Update.Record(_timer.Elapsed.TotalMilliseconds);
#endif
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
            var entry = _systems[index];

#if !ARCH_METRICS_DISABLED
            _timer.Restart();
            {
#endif

                entry.System.AfterUpdate(in t);

#if !ARCH_METRICS_DISABLED
            }
            _timer.Stop();
            entry.AfterUpdate.Record(_timer.Elapsed.TotalMilliseconds);
#endif
        }
    }

    /// <summary>
    ///     Disposes this <see cref="Group{T}"/> and all <see cref="ISystem{T}"/>'s within.
    /// </summary>
    public void Dispose()
    {
        foreach (var system in _systems)
        {
            system.Dispose();
        }
    }

    /// <summary>
    ///     Converts this <see cref="Group{T}"/> to a human readable string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        // List all system names
        var stringBuilder = new StringBuilder();
        foreach (var systemEntry in _systems)
        {
            stringBuilder.Append($"{systemEntry.System.GetType().Name},");
        }

        // Cut last `,`
        if (_systems.Count > 0)
        {
            stringBuilder.Length--;
        }
        
        return $"Group = {{ {nameof(Name)} = {Name}, Systems = {{ {stringBuilder} }} }} ";
    }

    /// <summary>
    ///     The struct <see cref="SystemEntry"/> represents the given <see cref="ISystem{T}"/> in the <see cref="Group{T}"/> with all its performance statistics.
    /// </summary>
    private readonly struct SystemEntry : IDisposable
    {
        public readonly ISystem<T> System;

#if !ARCH_METRICS_DISABLED
        public readonly Histogram<double> BeforeUpdate;
        public readonly Histogram<double> Update;
        public readonly Histogram<double> AfterUpdate;
#endif

        public void Dispose()
        {
            System.Dispose();
        }

        public SystemEntry(ISystem<T> system
#if !ARCH_METRICS_DISABLED
                , Meter meter
#endif
            )
        {
            var name = system.GetType().Name;
            System = system;

#if !ARCH_METRICS_DISABLED
            BeforeUpdate = meter.CreateHistogram<double>($"{name}.BeforeUpdate", unit: "millisecond");
            Update = meter.CreateHistogram<double>($"{name}.Update", unit: "millisecond");
            AfterUpdate = meter.CreateHistogram<double>($"{name}.AfterUpdate", unit: "millisecond");
#endif
        }
    }
}