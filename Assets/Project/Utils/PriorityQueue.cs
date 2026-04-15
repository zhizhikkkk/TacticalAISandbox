using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private readonly List<(TElement Element, TPriority Priority)> _heap = new List<(TElement, TPriority)>();

    public int Count => _heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        _heap.Add((element, priority));
        int i = _heap.Count-1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_heap[parent].Priority.CompareTo(_heap[i].Priority) <= 0)
            {
                break;
            }
            Swap(i, parent);
            i = parent;
        }

    }

    public TElement Dequeue()
    {
        if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty");

        TElement result = _heap[0].Element;
        int lastIndex = _heap.Count - 1;
        _heap.RemoveAt(lastIndex);

        int i = 0;
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left < _heap.Count && _heap[left].Priority.CompareTo(_heap[smallest].Priority) < 0)
            {
                smallest = left;
            }
            if (right <_heap.Count && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0)
            {
                smallest = right;
            }
            if (smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
        return result;
    }

    private void Swap(int a,int b)
    {
        var temp = _heap[a];
        _heap[a] = _heap[b];
        _heap[b] = temp;
    }
}