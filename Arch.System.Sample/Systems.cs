using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Arch.System.Sample;

/// <summary>
/// The movement system makes the entities move and bounce properly. 
/// </summary>
public partial class MovementSystem : BaseSystem<World, GameTime>
{
    private readonly Rectangle _viewport;
    public MovementSystem(World world, Rectangle viewport) : base(world) { _viewport = viewport;}

    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(ref Position pos, ref Velocity vel)
    {
        pos.Vector2 += Data.ElapsedGameTime.Milliseconds * vel.Vector2;
    }
    
    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Bounce(ref Position pos, ref Velocity vel)
    {
        if (pos.Vector2.X >= _viewport.X + _viewport.Width)
            vel.Vector2.X = -vel.Vector2.X;
            
        if (pos.Vector2.Y >= _viewport.Y + _viewport.Height)
            vel.Vector2.Y = -vel.Vector2.Y;
            
        if (pos.Vector2.X <= _viewport.X)
            vel.Vector2.X = -vel.Vector2.X;
            
        if (pos.Vector2.Y <= _viewport.Y)
            vel.Vector2.Y = -vel.Vector2.Y;
    }
}

/// <summary>
/// Color system, modifies each entities color slowly. 
/// </summary>
public partial class ColorSystem : BaseSystem<World, GameTime>
{
    public ColorSystem(World world) : base(world) {}

    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ChangeColor(ref Sprite sprite)
    {
        sprite.Color.R += (byte)(Data.ElapsedGameTime.TotalMilliseconds * 0.08);
        sprite.Color.G += (byte)(Data.ElapsedGameTime.TotalMilliseconds * 0.08);
        sprite.Color.B += (byte)(Data.ElapsedGameTime.TotalMilliseconds * 0.08);
    }
}

/// <summary>
/// The draw system, handles the drawing of entity sprites at their position. 
/// </summary>
public partial class DrawSystem : BaseSystem<World, GameTime>
{
    private readonly SpriteBatch _batch;
    public DrawSystem(World world, SpriteBatch batch) : base(world) { _batch = batch;}

    public override void BeforeUpdate(in GameTime t)
    {
        base.BeforeUpdate(in t);
        _batch.Begin();
    }

    [Update]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(ref Position position, ref Sprite sprite)
    {
        _batch.Draw(sprite.Texture2D, position.Vector2, sprite.Color);  // Draw
    }

    public override void AfterUpdate(in GameTime t)
    {
        base.AfterUpdate(in t);
        _batch.End();
    }
}

/// <summary>
///     The debug system, shows how you can combine source generated queries and default ones. 
/// </summary>
public partial class DebugSystem : BaseSystem<World, GameTime>
{
    private readonly QueryDescription _customQuery = new QueryDescription().WithAll<Position, Sprite>().WithNone<Velocity>();
    public DebugSystem(World world) : base(world) { }

    public override void Update(in GameTime t)
    {
        World.Query(in _customQuery, (in Entity entity) => Console.WriteLine($"Custom : {entity}"));  // Manual query
        PrintEntitiesWithoutVelocityQuery();  // Call source generated query, which calls the PrintEntitiesWithoutVelocity method
    }

    [Update]
    [All<Position, Sprite>, None<Velocity>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrintEntitiesWithoutVelocity(in Entity entity)
    {
        Console.WriteLine(entity);
    }
}