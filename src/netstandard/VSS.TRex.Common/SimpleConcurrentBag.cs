using System.Collections.Generic;

namespace VSS.TRex.Common
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
    /// The number of items contained in the simple concurrent bag
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Returns the allocated capacity of the internal container in terms of number of elements of type T"/>
    /// </summary>
    public int Capacity
    {
      get
      {
        lock (Items)
        {
          return Items.Count;
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
      lock (Items)
      {
        if (Count < Items.Count)
        {
          // Reuse an element in the list
          Items[Count++] = item; // There is another element being stored; include it in count
        }
        else
        {
          // All list elements are occupied, add the item as a new element in the list
          Items.Add(item);
          Count++;
        }
      }
    }

    /// <summary>
    /// Add an array of elements to the bag. The first [count] elements of the array will be added.
    /// </summary>
    /// <param name="itemArray"></param>
    /// <param name="itemCount"></param>
    public void Add(T[] itemArray, int itemCount)
    {
      lock (Items)
      {
        for (int i = 0; i < itemCount; i++)
        {
          if (itemArray[i] == null)
          {
            continue;
          }

          if (Count < Items.Count)
          {
            // Reuse an element in the list
            Items[Count++] = itemArray[i]; // There is another element being stored; include it in count
          }
          else
          {
            // All list elements are occupied, add the item as a new element in the list
            Items.Add(itemArray[i]);
            Count++;
          }
        }
      }
    }

    /// <summary>
    /// Remove an item from the concurrent bag. If no item is available to be removed then return value, true otherwise
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool TryTake(out T item)
    {
      lock (Items)
      {
        if (Count == 0)
        {
          item = null;
        }
        else
        {
          item = Items[--Count];
          Items[Count] = null;
        }

        return item != null;
      }
    }
  }
}
