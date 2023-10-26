namespace Arch.LowLevel.Tests;
using static Assert;

/// <summary>
///     Checks <see cref="UnsafeArray{T}"/> related methods.
/// </summary>
[TestFixture]
public class UnsafeArrayTest
{
    /// <summary>
    ///     Checks if <see cref="UnsafeArray{T}"/> is capable of allocating space and adding items.
    /// </summary>
    [Test]
    public void UnsafeArrayCreate()
    {
        using var array = new UnsafeArray<int>(3);
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;
        
        That(array.Count, Is.EqualTo(3));
    }

    [Test]
    public void UnsafeArrayEmptyIsEmpty()
    {
        var empty = UnsafeArray.Empty<long>();

        That(empty, Is.Empty);

        empty.Dispose();

        That(empty, Is.Empty);
    }

    [Test]
    public void UnsafeArrayFill()
    {
        var array = new UnsafeArray<int>(35);
        using (array)
        {
            UnsafeArray.Fill(ref array, 8);

            for (var i = 0; i < array.Length; i++)
                That(array[i], Is.EqualTo(8));
        }
    }

    [Test]
    public void UnsafeArrayCopy()
    {
        var src = new UnsafeArray<int>(15);
        var dst = new UnsafeArray<int>(6);
        using (src)
        using (dst)
        {
            for (var i = 0; i < src.Length; i++)
                src[i] = i;

            UnsafeArray.Fill(ref dst);
            UnsafeArray.Copy(ref src, 4, ref dst, 1, 4);

            Multiple(() =>
            {
                That(dst[0], Is.EqualTo(0));
                That(dst[1], Is.EqualTo(4));
                That(dst[2], Is.EqualTo(5));
                That(dst[3], Is.EqualTo(6));
                That(dst[4], Is.EqualTo(7));
                That(dst[5], Is.EqualTo(0));
            });
        }
    }

    [Test]
    public void UnsafeArrayResizeShrink()
    {
        var array = new UnsafeArray<int>(19);
        for (var i = 0; i < array.Length; i++)
            array[i] = i;

        var resized = UnsafeArray.Resize(ref array, 8);
        for (var i = 0; i < resized.Length; i++)
            That(resized[i], Is.EqualTo(i));

        resized.Dispose();
    }

    [Test]
    public void UnsafeArrayResizeGrow()
    {
        var array = new UnsafeArray<int>(8);
        for (var i = 0; i < array.Length; i++)
            array[i] = i;

        var resized = UnsafeArray.Resize(ref array, 19);
        for (var i = 0; i < array.Length; i++)
            That(resized[i], Is.EqualTo(i));

        resized.Dispose();
    }
}