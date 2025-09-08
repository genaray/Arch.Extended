using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using CommunityToolkit.HighPerformance;

namespace Arch.Relationships;

/// <summary>
///     The <see cref="SortedListEnumerator{TValue}"/> struct
///     is a enumerator to enumerate a passed <see cref="SortedList{TKey,TValue}"/> in an efficient way. 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public struct SortedListEnumerator<TValue> 
{
    private SortedList<Entity, TValue> sortedList;
    private int currentIndex;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="list">List.</param>
    public SortedListEnumerator(SortedList<Entity, TValue> list)
    {
        sortedList = list;
        currentIndex = -1;
    }

    /// <summary>
    /// Current.
    /// </summary>
    public KeyValuePair<Entity, TValue> Current
    {
        get
        {
            if (currentIndex == -1 || currentIndex >= sortedList.Count)
                throw new InvalidOperationException();
                
            var key = sortedList.Keys[currentIndex];
            var value = sortedList.Values[currentIndex];
            return new KeyValuePair<Entity, TValue>(key, value);
        }
    }
    
    /// <summary>
    /// Moves to the next element in the enumerator.
    /// </summary>
    public bool MoveNext()
    {
        currentIndex++;
        return currentIndex < sortedList.Count;
    }

    /// <summary>
    /// Resets the enumerator to its initial position.
    /// </summary>
    public void Reset()
    {
        currentIndex = -1;
    }
}