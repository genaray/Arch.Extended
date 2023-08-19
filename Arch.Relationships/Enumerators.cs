using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using CommunityToolkit.HighPerformance;

namespace Arch.Relationships;

/// <summary>
///     The <see cref="SortedListEnumerator{TKey,TValue}"/> struct
///     is a enumerator to enumerate a passed <see cref="SortedList{TKey,TValue}"/> in an efficient way. 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public struct SortedListEnumerator<TValue> 
{
    private SortedList<Entity, TValue> sortedList;
    private int currentIndex;

    public SortedListEnumerator(SortedList<Entity, TValue> list)
    {
        sortedList = list;
        currentIndex = -1;
    }

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
    
    public bool MoveNext()
    {
        currentIndex++;
        return currentIndex < sortedList.Count;
    }

    public void Reset()
    {
        currentIndex = -1;
    }
}