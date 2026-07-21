namespace Simple.BotUtils.Data;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A fixed-capacity circular buffer that keeps only the most recent <see cref="Limit"/> items.
/// Adding past the limit overwrites the oldest entry. Backed by a single array, so adds are O(1)
/// with no per-item allocation. Indexing and enumeration are oldest-first (index 0 = oldest).
/// </summary>
public class LastList<T> : IReadOnlyList<T>, ICollection<T>
{
    private readonly T[] buffer;
    private int head;    // index of the oldest item
    private int count;
    private int version; // bumped on every mutation, guards enumeration

    public LastList(int limit)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be at least 1");
        buffer = new T[limit];
    }

    /// <summary>Maximum number of items retained.</summary>
    public int Limit => buffer.Length;
    public int Count => count;
    bool ICollection<T>.IsReadOnly => false;

    /// <summary>Gets the item at <paramref name="index"/>, oldest first (0 = oldest).</summary>
    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)count) throw new ArgumentOutOfRangeException(nameof(index));
            return buffer[(head + index) % buffer.Length];
        }
    }

    /// <summary>Adds an item at the end; if already at <see cref="Limit"/>, the oldest item is dropped.</summary>
    public void Add(T item)
    {
        buffer[(head + count) % buffer.Length] = item;

        if (count == buffer.Length)
            head = (head + 1) % buffer.Length; // full: oldest just got overwritten, advance head
        else
            count++;

        version++;
    }

    /// <summary>Adds several items at the end, keeping only the most recent <see cref="Limit"/>.</summary>
    public void AddRange(IEnumerable<T> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        foreach (var v in values) Add(v);
    }

    public void Clear()
    {
        Array.Clear(buffer, 0, buffer.Length); // drop references so the GC can reclaim them
        head = 0;
        count = 0;
        version++;
    }

    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < count; i++)
        {
            if (comparer.Equals(buffer[(head + i) % buffer.Length], item)) return true;
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex + count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        int firstRun = Math.Min(count, buffer.Length - head);
        Array.Copy(buffer, head, array, arrayIndex, firstRun);
        if (count > firstRun) Array.Copy(buffer, 0, array, arrayIndex + firstRun, count - firstRun);
    }

    // Size is known, there is no need to allocate a LargeArrayBuilder
    public T[] ToArray()
    {
        var arr = new T[count];
        CopyTo(arr, 0);
        return arr;
    }

    public bool Remove(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < count; i++)
        {
            if (!comparer.Equals(buffer[(head + i) % buffer.Length], item)) continue;
            removeAt(i);
            return true;
        }
        return false;
    }

    private void removeAt(int logicalIndex)
    {
        int cap = buffer.Length;
        for (int i = logicalIndex; i < count - 1; i++)
        {
            buffer[(head + i) % cap] = buffer[(head + i + 1) % cap];
        }
        buffer[(head + count - 1) % cap] = default!; // release the vacated slot
        count--;
        version++;
    }

    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Allocation-free enumerator, oldest first.</summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly LastList<T> owner;
        private readonly int version;
        private int index;
        private T current;

        internal Enumerator(LastList<T> owner)
        {
            this.owner = owner;
            version = owner.version;
            index = 0;
            current = default!;
        }

        public readonly T Current => current;
        readonly object? IEnumerator.Current => current;

        public bool MoveNext()
        {
            if (version != owner.version) throw new InvalidOperationException("Collection was modified during enumeration");
            if (index >= owner.count)
            {
                current = default!;
                return false;
            }
            current = owner.buffer[(owner.head + index) % owner.buffer.Length];
            index++;
            return true;
        }

        public void Reset()
        {
            if (version != owner.version) throw new InvalidOperationException("Collection was modified during enumeration");
            index = 0;
            current = default!;
        }

        readonly void IDisposable.Dispose() { }
    }
}
