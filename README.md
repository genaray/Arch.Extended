# Arch.Extended
[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg?style=for-the-badge)](https://GitHub.com/Naereen/StrapDown.js/graphs/commit-activity)
[![Nuget](https://img.shields.io/nuget/v/Arch?style=for-the-badge)](https://www.nuget.org/packages/Arch/)
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

# Systems Code sample

The Arch.System package provides a number of useful classes to organize and structure queries. 
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

The Arch.System.SourceGenerator provides some code generation utils. 
With them, queries within systems can be written virtually by themselves and it saves some boilerplate code. 

The only thing you have to pay attention to is that the system class is partial and inherits from BaseSystem.
The attributes can be used to meaningfully describe what query to generate, and the query will always call the annotated method.

> Systems should lie within a namespace, the global one is NOT supported at the moment.

```cs
// Components ( ignore the formatting, this saves space )
public struct Position{ float X, Y };
public struct Velocity{ float Dx, Dy };

// BaseSystem provides several usefull methods for interacting and structuring systems
public partial class MovementSystem : BaseSystem<World, float>{

    public MovementSystem(World world) : base(world) {}
    
    // Generates a query and calls this annotated method for all entities with position and velocity components.
    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntity(ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
    }
    
    /// Generates a query and calls this method for all entities with position, velocity, player, mob, particle, either moving or idle and no dead component.
    /// All, Any, None are seperate attributes and do not require each other.
    [Update]
    [All<Player, Mob, Particle>, Any<Moving, Idle>, None<Dead>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntityWithConstraints(ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
    }
}
```

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
        MoveEntityQuery();  // Call source generated query, which calls the MoveEntity method
    }

    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveEntity(ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
    }
}
```
