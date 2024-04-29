using System.Runtime.CompilerServices;
using Arch.Bus;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.LowLevel;
using Arch.System;
using Arch.System.SourceGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Arch.Extended;

/// <summary>
///     The movement system makes the entities move and bounce properly. 
/// </summary>
public partial class MovementSystem : BaseSystem<World, GameTime>
{
    
    /// <summary>
    ///     A rectangle which specifies the viewport.
    ///     Needed so that the entities do not wander outside the viewport.
    /// </summary>
    private readonly Rectangle _viewport;
    
    /// <summary>
    ///     Creates a <see cref="MovementSystem"/> instance.
    /// </summary>
    /// <param name="world">The <see cref="World"/> used.</param>
    /// <param name="viewport">The games <see cref="Viewport"/>.</param>
    public MovementSystem(World world, Rectangle viewport) : base(world) { _viewport = viewport;}

    /// <summary>
    ///     Called for each <see cref="Entity"/> to move it.
    ///     The calling takes place through the source generated method "MoveQuery" on <see cref="BaseSystem{W,T}.Update"/>.
    /// </summary>
    /// <param name="time">The <see cref="GameTime"/>, passed by the "MoveQuery".</param>
    /// <param name="pos">The <see cref="Position"/> of the <see cref="Entity"/>. Passed by the "MoveQuery".</param>
    /// <param name="vel">The <see cref="Velocity"/> of the <see cref="Entity"/>. Passed by the "MoveQuery".</param>
    [Query(Parallel = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Move([Data] GameTime time, ref Position pos, ref Velocity vel)
    {
        pos.Vector2 += time.ElapsedGameTime.Milliseconds * vel.Vector2;
    }
    
    /// <summary>
    ///     Called for each <see cref="Entity"/> to move it.
    ///     The calling takes place through the source generated method "MoveQuery" on <see cref="BaseSystem{W,T}.Update"/>.
    /// </summary>
    /// <param name="pos">The <see cref="Position"/> of the <see cref="Entity"/>. Passed by the "MoveQuery".</param>
    /// <param name="vel">The <see cref="Velocity"/> of the <see cref="Entity"/>. Passed by the "MoveQuery".</param>
    [Query]
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
///     Color system, modifies each entities color slowly. 
/// </summary>
public partial class ColorSystem : BaseSystem<World, GameTime>
{
    /// <summary>
    ///     Creates an <see cref="ColorSystem"/> instance.
    /// </summary>
    /// <param name="world"></param>
    public ColorSystem(World world) : base(world) {}

    /// <summary>
    ///     Called for each <see cref="Entity"/> to change its color.
    ///     The calling takes place through the source generated method "ChangeColorQuery" on <see cref="BaseSystem{W,T}.Update"/>.
    /// </summary>
    /// <param name="time">The <see cref="GameTime"/> Passed by the "MoveQuery".</param>
    /// <param name="sprite">The <see cref="Sprite"/> of the <see cref="Entity"/>. Passed by the "ChangeColorQuery".</param>
    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ChangeColor([Data] GameTime time, ref Sprite sprite)
    {
        sprite.Color.R += (byte)(time.ElapsedGameTime.TotalMilliseconds * 0.08);
        sprite.Color.G += (byte)(time.ElapsedGameTime.TotalMilliseconds * 0.08);
        sprite.Color.B += (byte)(time.ElapsedGameTime.TotalMilliseconds * 0.08);
    }
}

/// <summary>
///     The draw system, handles the drawing of <see cref="Entity"/> sprites at their position. 
/// </summary>
public partial class DrawSystem : BaseSystem<World, GameTime>
{
    /// <summary>
    ///     The <see cref="SpriteBatch"/> used for drawing all <see cref="Entity"/>s.
    /// </summary>
    private readonly SpriteBatch _batch;
    
    /// <summary>
    ///     Creates a <see cref="DrawSystem"/> instance.
    /// </summary>
    /// <param name="world">The <see cref="World"/> used.</param>
    /// <param name="batch">The <see cref="SpriteBatch"/> used.</param>
    public DrawSystem(World world, SpriteBatch batch) : base(world) { _batch = batch;}

    /// <summary>
    ///     Is called before the <see cref="BaseSystem{W,T}.Update"/> to start with the <see cref="SpriteBatch"/> recording.
    /// </summary>
    /// <param name="t">The <see cref="GameTime"/>.</param>
    public override void BeforeUpdate(in GameTime t)
    {
        base.BeforeUpdate(in t);
        _batch.Begin();
    }

    /// <summary>
    ///     Called for each <see cref="Entity"/> to draw it.
    ///     The calling takes place through the source generated method "DrawQuery" on <see cref="BaseSystem{W,T}.Update"/>.
    /// </summary>
    /// <param name="position">The <see cref="Position"/> of the <see cref="Entity"/>. Passed by the "DrawQuery".</param>
    /// <param name="sprite">The <see cref="Sprite"/> of the <see cref="Entity"/>. Passed by the "DrawQuery".</param>
    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(ref Position position, ref Sprite sprite)
    {
        _batch.Draw(sprite.Texture2D, position.Vector2, sprite.Color);  // Draw
    }

    /// <summary>
    ///     Is called after the <see cref="BaseSystem{W,T}.Update"/> to stop the <see cref="SpriteBatch"/> recording.
    /// </summary>
    /// <param name="t">The <see cref="GameTime"/>.</param>
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
    /// <summary>
    ///     A custom <see cref="QueryDescription"/> which targets <see cref="Entity"/>s with <see cref="Position"/> and <see cref="Sprite"/> without <see cref="Velocity"/>.
    /// </summary>
    private readonly QueryDescription _customQuery = new QueryDescription().WithAll<Position, Sprite>().WithNone<Velocity>();

    /// <summary>
    ///     Creates a new <see cref="DebugSystem"/> instance. 
    /// </summary>
    /// <param name="world">The <see cref="World"/> used.</param>
    public DebugSystem(World world) : base(world)
    {
        Hook();
    }

    /// <summary>
    ///     Implements <see cref="BaseSystem{W,T}.Update"/> to call the custom Query and the source generated one. 
    /// </summary>
    /// <param name="t">The <see cref="GameTime"/>.</param>
    public override void Update(in GameTime t)
    {
        World.Query(in _customQuery, entity => Console.WriteLine($"Custom : {entity}"));  // Manual query
        PrintEntitiesWithoutVelocityQuery(World);  // Call source generated query, which calls the PrintEntitiesWithoutVelocity method
    }

    /// <summary>
    ///     Called for each <see cref="Entity"/> with <see cref="Position"/> and <see cref="Sprite"/> without <see cref="Velocity"/> to print it.
    ///     The calling takes place through the source generated method "PrintEntitiesWithoutVelocityQuery" on <see cref="DebugSystem.Update"/>.
    /// </summary>
    /// <param name="en">The <see cref="Entity"/>. Passed by the "PrintEntitiesWithoutVelocityQuery", you can also pass components or data parameters as usual.</param>
    [Query]
    [All<Position, Sprite>, None<Velocity>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrintEntitiesWithoutVelocity(in Entity en)
    {
        Console.WriteLine(en);
    }

    /// <summary>
    ///     Receives dispatched keyboard events and if the a key was pressed, prints it.
    ///     Runs before any other event receiver of this kind.
    /// </summary>
    /// <param name="tuple">The event listened to.</param>
    [Event(order: 0)]
    public void OnKeyboardEventPrint(ref (World world, KeyboardState state) tuple)
    {
        if (!tuple.state.IsKeyDown(Keys.A)) return;
        Console.WriteLine($"Key a was pressed.");
    }
}

/// <summary>
/// A event handler class using the source generated eventbus to intercept and react to events to decouple logic. 
/// </summary>
public static partial class EventHandler
{

    /// <summary>
    /// Listens for <see cref="ValueTuple{T1,T2}"/> events with a <see cref="World"/> and <see cref="KeyboardState"/> to check if the delte key was pressed.
    /// If thats the case, it will remove the <see cref="Velocity"/> component from all of them. 
    /// </summary>
    /// <param name="tuple"></param>
    [Event(order: 1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnDeleteStopEntities(ref (World world, KeyboardState state) tuple)
    {
        if (!tuple.state.IsKeyDown(Keys.Delete)) return;
        
        // Query for velocity entities and remove their velocity to make them stop moving. 
        var queryDesc = new QueryDescription().WithAll<Velocity>();
        tuple.world.Query(in queryDesc, entity => entity.Remove<Velocity>());
    }
}