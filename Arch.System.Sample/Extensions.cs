using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Arch.System.Sample;

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