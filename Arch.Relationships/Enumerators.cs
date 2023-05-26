using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace Arch.Relationships;

/// <summary>
///     The <see cref="SortedListEnumerator{TKey,TValue}"/> struct
///     is a enumerator to enumerate a passed <see cref="SortedList{TKey,TValue}"/> in an efficient way. 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public struct SortedListEnumerator<TKey, TValue> 
{
    private SortedList<TKey, TValue> sortedList;
    private int currentIndex;

    public SortedListEnumerator(SortedList<TKey, TValue> list)
    {
        sortedList = list;
        currentIndex = -1;
    }

    public KeyValuePair<TKey, TValue> Current
    {
        get
        {
            if (currentIndex == -1 || currentIndex >= sortedList.Count)
                throw new InvalidOperationException();
                
            var key = sortedList.Keys[currentIndex];
            var value = sortedList.Values[currentIndex];
            return new KeyValuePair<TKey, TValue>(key, value);
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