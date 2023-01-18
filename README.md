# Arch.Extended
[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg?style=for-the-badge)](https://GitHub.com/Naereen/StrapDown.js/graphs/commit-activity)
[![Nuget](https://img.shields.io/nuget/v/Arch?style=for-the-badge)](https://www.nuget.org/packages/Arch.System/)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)

Extensions for [Arch](https://github.com/genaray/Arch) with some useful features like Systems, Source Generator and Utils.
 
- 🛠️ **_Productive_** >  Adds some useful tools and features to the main repository!
- ☕️ **_SIMPLE_** >  Works easily, reliably and understandably!
- 💪 _**MAINTAINED**_ > It's actively being worked on, maintained, and supported!
- 🚢 _**SUPPORT**_ > Supports .NetStandard 2.1, .Net Core 6 and 7 and therefore you may use it with Unity or Godot!

# Features & Tools
- ⚙️ **_Systems_** > By means of systems, it is now easy to organize, reuse and arrange queries. 
- ✍️ **_Source Generator_** > Declarative syntax using attributes and source generator, let your queries write themselves! 


# Fully featured Code sample

With this package you are able to write and group queries and systems for Arch automatically.
And all this with the best possible performance

```cs
// Components ( ignore the formatting, this saves space )
public struct Position{ float X, Y };
public struct Velocity{ float Dx, Dy };

// BaseSystem provides several usefull methods for interacting and structuring systems
public class MovementSystem : BaseSystem<World, float>
{
    public MovementSystem(World world) : base(world) {}
    
    // Generates a query and calls that one automatically on BaseSystem.Update
    [Query]
    public void Move([Data] in float time, ref Position pos, ref Velocity vel)
    {
        pos.X += time * vel.X;
        pos.Y += time * vel.Y;
    }
    
    // Generates and filters a query and calls that one automatically on BaseSystem.Update in order
    [Query]
    [All<Player, Mob>, Any<Idle, Moving>, None<Alive>]
    public void ResetVelocity(ref Velocity vel)
    {
        vel = new Velocity{ X = 0, Y = 0 };
    }
}

public class Game 
{
    public static void Main(string[] args) 
    {     
        var deltaTime = 0.05f; // This is mostly given by engines, frameworks
        
        // Create a world and a group of systems which will be controlled 
        var world = World.Create();
        var _systems = new Group<float>(
            new MovementSystem(world),   // Run in order
            new MyOtherSystem(...),
            ...
        );
      
        _systems.Initialize();                  // Inits all registered systems
        _systems.BeforeUpdate(in deltaTime);    // Calls .BeforeUpdate on all systems ( can be overriden )
        _systems.Update(in deltaTime);          // Calls .Update on all systems ( can be overriden )
        _systems.AfterUpdate(in deltaTime);     // Calls .AfterUpdate on all System ( can be overriden )
        _systems.Dispose();                     // Calls .Dispose on all systems ( can be overriden )
    }
}
```
# Contents
- [Systems Code sample](#systems-code-sample)
- [Systems source generator](#systems-source-generator)
    * [Syntax](#syntax)
    * [Query Methods in BaseSystem](#query-methods-in-basesystem)
    * [Query Methods in custom classes](#query-methods-in-custom-classes)

# Systems Code sample

The `Arch.System` package provides a number of useful classes to organize and structure queries. 
These are organized into "systems" and can also be grouped.

The example below demonstrates a slightly larger code sample

```cs
// Components ( ignore the formatting, this saves space )
public struct Position{ float X, Y };
public struct Velocity{ float Dx, Dy };

// BaseSystem provides several usefull methods for interacting and structuring systems
public class MovementSystem : BaseSystem<World, float>{

    private QueryDescription _desc = new QueryDescription().WithAll<Position, Velocity>();
    public MovementSystem(World world) : base(world) {}
    
    // Can be called once per frame
    public override void Update(in float deltaTime)
    {
        // Run query, can also run multiple queries inside the update
        World.Query(in _desc, (ref Position pos, ref Velocity vel) => {
            pos.X += vel.X;
            pos.Y += vel.Y;
        });  
    }
}

public class Game 
{
    public static void Main(string[] args) 
    {     
        var deltaTime = 0.05f; // This is mostly given by engines, frameworks
        
        // Create a world and a group of systems which will be controlled 
        var world = World.Create();
        var _systems = new Group<float>(
            new MovementSystem(world),   // Run in order
            new MyOtherSystem(...),
            ...
        );
      
        _systems.Initialize();                  // Inits all registered systems
        _systems.BeforeUpdate(in deltaTime);    // Calls .BeforeUpdate on all systems ( can be overriden )
        _systems.Update(in deltaTime);          // Calls .Update on all systems ( can be overriden )
        _systems.AfterUpdate(in deltaTime);     // Calls .AfterUpdate on all System ( can be overriden )
        _systems.Dispose();                     // Calls .Dispose on all systems ( can be overriden )
    }
}
```

# Systems source generator

The `Arch.System.SourceGenerator` package provides some code generation utils. 
With them, queries can be written virtually by themselves which saves some boilerplate code. 

Query methods can be generated in all classes as long as they are partial. However it makes most sense in the `BaseSystem`.
The attributes can be used to meaningfully describe what query to generate, and the query will always call the annotated method.

The generated methods can also be called manually.

> Classes using the source generation should be within a namespace, the global one is NOT supported at the moment.

## Syntax

`Arch.System` provides some attributes that can be used for code generation. 
These can be arranged and used arbitrarily.

- `[Query]` > Update marks some method so that the code generator makes a query out of it.
- `[All<T0...T25>]` > Mirrors `QueryDescription.All`, defines which components an entity needs.
- `[Any<T0...T25>]` > Mirrors `QueryDescription.Any`, defines that an entity needs at least one of the components.
- `[None<T0...T25>]` > Mirrors `QueryDescription.None`, defines that an entity should have none of those components.
- `[Exclusive<T0...T25>]` > Mirrors `QueryDescription.Exclusive`, defines an exclusive set of entity components.
- `[Data]` > Marks a method parameter and specifies that this type should be passed through the query.

`Query` is always required. 
`All`, `Any`, `None`, `Exclusive`, `Data` are optional and independent from each other. 

Let us now look at some example.

## Query Methods in BaseSystem

It makes most sense to use the generator directly in the `BaseSystem`. 
If this is the case, the `BaseSystem.Update` method is automatically generated and the queries are called in order.

```cs
// Components ( ignore the formatting, this saves space )
public struct Position{ float X, Y };
public struct Velocity{ float Dx, Dy };

// BaseSystem provides several usefull methods for interacting and structuring systems
public partial class MovementSystem : BaseSystem<World, float>{

    public MovementSystem(World world) : base(world) {}
    
    // Generates a query and calls this annotated method for all entities with position and velocity components.
    [Query]  // Marks method inside BaseSystem for source generation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntity([Data] ref float time, ref Position pos, ref Velocity vel)  // Entity requires atleast those components. "in Entity" can also be passed. 
    {
        pos.X += time * vel.X;
        pos.Y += time * vel.Y;
    }
    
    /// Generates a query and calls this method for all entities with velocity, player, mob, particle, either moving or idle and no dead component.
    /// All, Any, None are seperate attributes and do not require each other.
    [Query]
    [All<Player, Mob, Particle>, Any<Moving, Idle>, None<Alive>]  // Adds filters to the source generation to adress certain entities. 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StopDeadEntities(ref Velocity vel)
    {
        vel.X = 0;
        vel.Y = 0;
    }
}
```

If a parameter of a method within a basesystem is marked as `Data`, then `BaseSystem.Data` is passed directly to the query. In the example above you can see this at the time: `float`.

If you use the source generator and its attributes in a class that does not override `BaseSystem.Update`, this method is implemented by the source generator itself.
If the method is already implemented by the user, only "query" methods are generated, which you can call yourself.

```csharp
public partial class MovementSystem : BaseSystem<World, GameTime>
{
    private readonly QueryDescription _customQuery = new QueryDescription().WithAll<Position, Velocity>();
    public DebugSystem(World world) : base(world) { }

    public override void Update(in GameTime t)
    {
        World.Query(in _customQuery, (in Entity entity) => Console.WriteLine($"Custom : {entity}"));  // Manual query
        MoveEntityQuery(World);  // Call source generated query, which calls the MoveEntity method
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntity(ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
    }
}
```

## Query Methods in custom classes

Queries can be generated in any class, in the following example a move and a hit query is generated and used for the class.
This is useful to group and reuse queries.

```csharp
// Class which will generate 
public partial class MyQueries{

    [Query]  // Marks method inside BaseSystem for source generation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntities([Data] in float time, ref Position pos, ref Velocity vel){
        pos.X += time * vel.X;
        pos.Y += time * vel.Y;
    }
    
    [Query]  // Marks method inside BaseSystem for source generation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DamageEntities(ref Hitted hit, ref Health health){
        health.value -= hit.value;
    }
    
    ... other Queries
}

// Instantiate class and call the generated methods 
var myQueries = new MyQueries();
myQueries.MoveEntitiesQuery(someWorld, 10.0f);  // World is always required for the generated method, 10.0f is the [Data] parameter
myQueries.DamageEntitiesQuery(someWorld);
```

The same works for static classes, whose performance is better.
The advantage is that no class instance is needed.

```csharp
// Class which will generate 
public static partial class MyQueries{

    [Query]  // Marks method inside BaseSystem for source generation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MoveEntities([Data] in float time, ref Position pos, ref Velocity vel){
        pos.X += time * vel.X;
        pos.Y += time * vel.Y;
    }
    
    [Query]  // Marks method inside BaseSystem for source generation.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DamageEntities(ref Hitted hit, ref Health health){
        health.value -= hit.value;
    }
    
    ... other Queries
}

// Instantiate class and call the generated methods 
MyQueries.MoveEntitiesQuery(someWorld, 10.0f);  // World is always required for the generated method, 10.0f is the [Data] parameter
MyQueries.DamageEntitiesQuery(someWorld);
```
