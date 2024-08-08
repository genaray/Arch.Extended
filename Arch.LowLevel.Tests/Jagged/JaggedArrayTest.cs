using System.Runtime.CompilerServices;
using Arch.LowLevel.Jagged;

namespace Arch.LowLevel.Tests.Jagged;
using static Assert;

/// <summary>
///     Checks <see cref="JaggedArray{T}"/>  related methods.
/// </summary>
[TestFixture]
public class JaggedArrayTest
{
    
    /// <summary>
    ///     Checks if <see cref="JaggedArray{T}"/> is capable of adding items correctly.
    /// </summary>
    [Test]
    public void Add([Values(256,512,1024,2048,4096)] int capacity)
    {
        // Check add
        var jaggedArray = new JaggedArray<int>(16000/Unsafe.SizeOf<int>(), -1, capacity);
        
        // adding
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            jaggedArray.Add(index, index);
        }
 
        // Checking
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            var item = jaggedArray[index];
            That(item, Is.EqualTo(index));
        }
        
        That(jaggedArray.Capacity, Is.GreaterThan(capacity));
    }
    
    [Test]
    public void TryGetValue([Values(256,512,1024,2048,4096)] int capacity)
    {
        // Initialize the JaggedArray
        var jaggedArray = new JaggedArray<int>(16000/Unsafe.SizeOf<int>(), -1, capacity);
    
        // Add elements to the array
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            jaggedArray.Add(index, index);
        }

        // Check values using TryGetValue
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            var found = jaggedArray.TryGetValue(index, out int value);
            That(found, Is.True);
            That(value, Is.EqualTo(index));
        }

        // Check for values out of bounds
        var outOfBoundsFound = jaggedArray.TryGetValue(jaggedArray.Capacity, out int _);
        That(outOfBoundsFound, Is.False);
    }
    
    [Test]
    public void TryGetValueRef([Values(256,512,1024,2048,4096)] int capacity)
    {
        // Initialize the JaggedArray
        var jaggedArray = new JaggedArray<int>(16000/Unsafe.SizeOf<int>(), -1, capacity);
    
        // Add elements to the array
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            jaggedArray.Add(index, index);
        }

        // Check values using TryGetValue
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            bool found;
            ref var value = ref jaggedArray.TryGetValue(index, out found);
            That(found, Is.True);
            That(value, Is.EqualTo(index));
        }

        // Check for values out of bounds
        ref var outOfBoundsValue = ref jaggedArray.TryGetValue(jaggedArray.Capacity, out bool outOfBoundsFound);
        That(outOfBoundsFound, Is.False);
    }

    
    /// <summary>
    ///     Checks if <see cref="JaggedArray{T}"/> is capable of adding items correctly.
    /// </summary>
    [Test]
    public void Remove([Values(256,512,1024,2048,4096)] int capacity)
    {
        // Check add
        var jaggedArray = new JaggedArray<int>(16000/Unsafe.SizeOf<int>(), -1, capacity);
        
        // Adding
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            jaggedArray.Add(index, index);
        }
 
        // Removing
        for (var index = jaggedArray.Capacity-1; index >= 0; index--)
        {
            jaggedArray.Remove(index);
        }
        
        // Checking
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            var item = jaggedArray[index];
            That(item, Is.EqualTo(-1));
        }
    }
    
    /// <summary>
    ///     Checks if <see cref="JaggedArray{T}"/> is capable of adding items correctly.
    /// </summary>
    [Test]
    public void TrimExcess([Values(2560,5120,10240)] int capacity)
    {
        // Check add
        var jaggedArray = new JaggedArray<int>(16000/Unsafe.SizeOf<int>(), -1, capacity);
        
        // Adding
        for (var index = 0; index < jaggedArray.Capacity; index++)
        {
            jaggedArray.Add(index, index);
        }
 
        // Removing half of items
        for (var index = jaggedArray.Capacity-1; index >= jaggedArray.Capacity/2; index--)
        {
            jaggedArray.Remove(index);
        }

        var buckets = jaggedArray.Buckets;
        jaggedArray.TrimExcess();
        That(jaggedArray.Buckets, Is.EqualTo((buckets + 2 - 1)/2));
        
        // Checking first half still having the desired value
        for (var index = 0; index < jaggedArray.Capacity/2; index++)
        {
            var item = jaggedArray[index];
            That(item, Is.EqualTo(index));
        }
    }
}