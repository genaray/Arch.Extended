using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Arch.Extended;

public static class TextureExtensions
{
    /// <summary>
    ///     Creates a square texture and returns it.
    /// </summary>
    /// <param name="graphicsDevice"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static Texture2D CreateSquareTexture(GraphicsDevice graphicsDevice, int size)
    {
        var texture = new Texture2D(graphicsDevice, size, size);
        var data = new Color[size*size];
        for(var i=0; i < data.Length; ++i) data[i] = Color.White;
        texture.SetData(data);

        return texture;
    }
}

public static class RandomExtensions
{
    /// <summary>
    ///     Creates a random <see cref="Vector2"/> inside the <see cref="Rectangle"/> and returns it.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance.</param>
    /// <param name="rectangle">A <see cref="Rectangle"/> in which a <see cref="Vector2"/> is generated. </param>
    /// <returns>The generated <see cref="Vector2"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NextVector2(this Random random, in Rectangle rectangle)
    {
        return new Vector2(random.Next(rectangle.X, rectangle.X+rectangle.Width), random.Next(rectangle.Y, rectangle.Y+rectangle.Height));
    }
    
    /// <summary>
    ///     Creates a random <see cref="Vector2"/> between two floats.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>A <see cref="Vector2"/> between those to floats.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NextVector2(this Random random, float min, float max)
    {
        return new Vector2((float)(random.NextDouble() * (max - min) + min), (float)(random.NextDouble() * (max - min) + min));
    }
    
    /// <summary>
    ///     Creates a random <see cref="Color"/>.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance.</param>
    /// <returns>A <see cref="Color"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color NextColor(this Random random)
    {
        return new Color(random.Next(0,255),random.Next(0,255),random.Next(0,255));
    }
}