using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;

namespace Test;

public record struct Position(float X, float Y);
public record struct Velocity(float X, float Y);

public partial class MovemmentSystem : BaseSystem<World,float>
{
    public MovemmentSystem(World world) : base(world) {}

    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MovementWithEntity(in Entity entity, ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
        
        Console.WriteLine($"Updated {entity}");
    }
    
    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MovementWithout(ref Position pos, ref Velocity vel)
    {
        pos.X += vel.X;
        pos.Y += vel.Y;
        
        Console.WriteLine($"Updated :)");
    }
    
    [Update]
    [All<Position, Velocity>, Any<Position>, Any<int>, None<long>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AttributeTest(in Entity entity)
    {
        var refs = entity.Get<Position, Velocity>();
        refs.t0.X += refs.t1.X;
        refs.t0.Y += refs.t1.Y;
        
        Console.WriteLine($"Updated {entity} with attributes only :) ");
    }
}