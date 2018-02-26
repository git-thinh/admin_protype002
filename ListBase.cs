using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web
{
    public class ListBase<T> 
    {
        private static List<T> list = new List<T>() { };

        public int Count()
        {
            int k = 0;
            lock (list) k = list.Count;
            return k;
        }

        public void Clear()
        {
            lock (list) list.Clear();
        }

        public void Add(T item)
        {
            lock (list)
                list.Add(item);
        }

        public T[] Find(Func<T, bool> condition)
        {
            T[] a = new T[] { };
            lock (list)
                a = list.Where(condition).ToArray();
            return a;
        }

        public T FindItem(Func<T, bool> condition)
        {
            T item = default(T);
            lock (list)
                item = list.Where(condition).Take(1).SingleOrDefault();
            return item;
        }

        public void UpdateByCondition(T itemNew, Func<T, bool> condition)
        {
            lock (list)
            {
                int index = FindIndex(list, condition);
                if (index != -1)
                    list[index] = itemNew;
            }
        }

        public void RemoveByCondition(Func<T, bool> condition)
        {
            lock (list)
            {
                int index = FindIndex(list, condition);
                if (index != -1)
                    list.RemoveAt(index);
            }
        }

        public T[] GetAll()
        {
            T[] a = new T[] { };
            lock (list)
                a = list.ToArray();
            return a;
        }
         
        public int FindIndex(IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            lock (list)
            {
                foreach (var item in items)
                {
                    if (predicate(item)) return retVal;
                    retVal++;
                }
            }
            return -1;
        }
    }
}