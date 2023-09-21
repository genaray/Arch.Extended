using System.Runtime.Serialization;
using Arch.AOT.SourceGenerator;
using Arch.Core;
using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Arch.Extended;

/// <summary>
///     The position of an entity.
/// </summary>
public struct Position
{

    /// <summary>
    ///     Its position.
    /// </summary>
    public Vector2 Vector2;
    
    /// <summary>
    ///     Constructs a new <see cref="Position"/> instance.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    public Position(float x, float y)
    {
        Vector2 = new Vector2(x, y);
    }
    
    /// <summary>
    ///     Constructs a new <see cref="Position"/> instance.
    /// <remarks>Mostly required for <see cref="MessagePack"/>.</remarks>
    /// </summary>
    /// <param name="vector2">The <see cref="Vector2"/>, the position.</param>
    public Position(Vector2 vector2)
    {
        Vector2 = vector2;
    }
};

/// <summary>
///     The velocity of an entity.
/// </summary>
public struct Velocity
{
    
    /// <summary>
    ///     Its velocity.
    /// </summary>
    public Vector2 Vector2;
    
    /// <summary>
    ///     Constructs a new <see cref="Velocity"/> instance.
    /// </summary>
    /// <param name="x">The x velocity.</param>
    /// <param name="y">The y velocity.</param>
    public Velocity(float x, float y)
    {
        Vector2 = new Vector2(x, y);
    }
    
    /// <summary>
    ///     Constructs a new <see cref="Velocity"/> instance.
    /// <remarks>Mostly required for <see cref="MessagePack"/>.</remarks>
    /// </summary>
    /// <param name="vector2">The <see cref="Vector2"/>, the velocity.</param>
    public Velocity(Vector2 vector2)
    {
        Vector2 = vector2;
    }
}

/// <summary>
/// The sprite/texture of an entity. 
/// </summary>
public struct Sprite
{
    /// <summary>
    ///     The <see cref="Texture2D"/> used.
    /// </summary>
    [IgnoreDataMember]
    public Texture2D Texture2D;

    /// <summary>
    ///     The id of the texture, for serialisation. 
    /// </summary>
    public byte TextureId;
    
    /// <summary>
    ///     The <see cref="Color"/> used. 
    /// </summary>
    public Color Color;

    /// <summary>
    ///     Constructs a new <see cref="Sprite"/> instance.
    /// </summary>
    /// <param name="texture2D">Its <see cref="Texture2D"/>.</param>
    /// <param name="color">Its <see cref="Color"/>.</param>
    public Sprite(Texture2D texture2D, Color color)
    {
        Texture2D = texture2D;
        Color = color;
    }
}
