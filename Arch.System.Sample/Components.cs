
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Arch.System.Sample;

/// <summary>
///     The position of an entity.
/// </summary>
/// <param name="Vector2">Its position.</param>
public struct Position
{

    public Vector2 Vector2;
    
    public Position(float x, float y)
    {
        Vector2 = new Vector2(x, y);
    }
};

/// <summary>
///     The velocity of an entity.
/// </summary>
/// <param name="Vector2">Its velocity.</param>
public struct Velocity
{

    public Vector2 Vector2;
    
    public Velocity(float x, float y)
    {
        Vector2 = new Vector2(x, y);
    }
}

/// <summary>
/// The sprite/texture of an entity. 
/// </summary>
/// <param name="Texture2D">Its texture.</param>
/// <param name="Color">Its color.</param>
public struct Sprite
{
    public Texture2D Texture2D;
    public Color Color;

    public Sprite(Texture2D texture2D, Color color)
    {
        Texture2D = texture2D;
        Color = color;
    }
}
