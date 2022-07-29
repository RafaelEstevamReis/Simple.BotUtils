using System.Collections;
using System.Collections.Generic;

namespace Simple.BotUtils.Data
{
    /// <summary>
    /// A simples linked list that removes the first ones to limit the list size
    /// </summary>
    public class LastList<T> : IEnumerable<T>, ICollection<T>
    {
        public int Limit { get; }

        LinkedList<T> list;
        public LastList(int limit)
        {
            list = new LinkedList<T>();
            Limit = limit;
        }

        public int Count
            => list.Count;

        /// <summary>
        /// Add an element at the end, and removes the first if is bigger than Limit
        /// </summary>
        public void Add(T item)
        {
            list.AddLast(item);
            if (list.Count > Limit) list.RemoveFirst();
        }
        /// <summary>
        /// Add elements to the end, and removes the first ones if bigger than Limit
        /// </summary>
        public void AddRange(IEnumerable<T> values)
        {
            foreach (var v in values) list.AddLast(v);
            while (list.Count > Limit) list.RemoveFirst();
        }

        public void Clear()
            => list.Clear();
        public bool Contains(T item)
            => list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex)
            => list.CopyTo(array, arrayIndex);
        public bool Remove(T item)
            => list.Remove(item);

        bool ICollection<T>.IsReadOnly
            => false;

        public IEnumerator<T> GetEnumerator()
            => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => list.GetEnumerator();
    }
}
