using System.ComponentModel.DataAnnotations;

namespace Arch.LowLevel.Tests;
using static Assert;

/// <summary>
///     Checks <see cref="Array{T}"/> related methods.
/// </summary>
[TestFixture]
public class ArrayTest
{
    /// <summary>
    ///     Checks if <see cref="Array{T}"/> is capable of allocating space and adding items.
    /// </summary>
    [Test]
    public void ArrayCreate()
    {
        var array = new Array<int>(3);
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;
        
        That(array.Count, Is.EqualTo(3));
    }

    [Test]
    public void ArrayEnumerator()
    {
        var array = new Array<int>(3);
        array[0] = 1;
        array[1] = 2;
        array[2] = 3;

        var count = 1;
        foreach (var item in array)
            That(item, Is.EqualTo(count++));
    }

    [Test]
    public void ArrayEmptyIsEmpty()
    {
        var empty = Array.Empty<long>();
        That(empty, Is.Empty);
    }

    [Test]
    public void ArrayFill()
    {
        var array = new Array<int>(35);
        Array.Fill(ref array, 8);

        for (var i = 0; i < array.Length; i++)
            That(array[i], Is.EqualTo(8));
    }

    [Test]
    public void ArrayCopy()
    {
        var src = new Array<int>(15);
        var dst = new Array<int>(6);

        for (var i = 0; i < src.Length; i++)
            src[i] = i;

        Array.Fill(ref dst);
        Array.Copy(ref src, 4, ref dst, 1, 4);

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

    [Test]
    public void ArrayResizeShrink()
    {
        var array = new Array<int>(19);
        for (var i = 0; i < array.Length; i++)
            array[i] = i;

        var resized = Array.Resize(ref array, 8);
        for (var i = 0; i < resized.Length; i++)
            That(resized[i], Is.EqualTo(i));
    }

    [Test]
    public void ArrayResizeGrow()
    {
        var array = new Array<int>(8);
        for (var i = 0; i < array.Length; i++)
            array[i] = i;

        var resized = Array.Resize(ref array, 19);
        for (var i = 0; i < array.Length; i++)
            That(resized[i], Is.EqualTo(i));
    }

    [Test]
    public void ArrayEquals()
    {
        var a = new Array<int>(8);
        var b = a;

        That(a, Is.EqualTo(b));
        That(a == b, Is.True);
    }

    [Test]
    public void ArrayNotEquals()
    {
        var a = new Array<int>(8);
        var b = new Array<int>(8);

        That(a, Is.Not.EqualTo(b));
        That(a != b, Is.True);
    }
}