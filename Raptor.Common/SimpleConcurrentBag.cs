using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Common
{
    /// <summary>
    /// Provides a very simple implementation of a concurrent bag that holds a collection of class instances. It provides no
    /// ordering guarantees, though this implementation will function semantically as a LIFO queue.
    /// </summary>
    public class SimpleConcurrentBag<T> where T : class
    {
        /// <summary>
        /// The internal container holding elements in the list
        /// </summary>
        private List<T> Items = new List<T>();

        /// <summary>
        /// Internal private counter for the number of elements in the list that contain elements
        /// </summary>
        private int count = 0;

        /// <summary>
        /// The number of 
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Returns the allocated capacity of the internal container in terms of number of elements of type T"/>
        /// </summary>
        public int Capacity
        {
            get
            {
                Monitor.Enter(Items);
                try
                {
                    return Items.Count();
                }
                finally
                {
                    Monitor.Exit(Items);
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleConcurrentBag()
        {
        }

        /// <summary>
        /// Add a element to the bag
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            Monitor.Enter(Items);
            try
            {
                if (count < Items.Count)
                {
                    // Reuse an element in the list
                    Items[count++] = item; // There is another element being stored; include it in count
                }
                else
                {
                    // All list elements are occupied, add the item as a new element in the list
                    Items.Add(item);
                    count++;
                }
            }
            finally
            {
                Monitor.Exit(Items);
            }
        }

        /// <summary>
        /// Remove an item from the concurrent bag. If no item is available to be removed then return falue, true otherwise
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryTake(out T item)
        {
            Monitor.Enter(Items);
            try
            {
                if (count == 0)
                {
                    item = null;
                }
                else
                {
                    item = Items[--count];
                    Items[count] = null;
                }

                return item != null;
            }
            finally
            {
                Monitor.Exit(Items);
            }
        }
    }
}
