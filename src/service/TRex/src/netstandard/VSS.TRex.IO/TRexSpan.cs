using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Defines a span of elements within a larger array of elements
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct TRexSpan<T>
  {
    /// <summary>
    /// Is the span allocated in a slab allocated buffer?
    /// </summary>
    public bool PoolAllocated;

    /// <summary>
    /// The offset within Elements array that the first element in the span occurs
    /// </summary>
    public int Offset;

    /// <summary>
    /// The number of defined elements present in the space Count is less than or equal to Length
    /// </summary>
    public int Count;

    /// <summary>
    /// The capacity of the span
    /// </summary>
    public int Capacity;

    /// <summary>
    /// The reference to the slab allocated array that this span's elements are contained
    /// </summary>
    public T[] Elements;

    /* Note: TR deliberately does not provide an indexer as it can contain structs which causes issues with
  return of copies of the struct. GetElement/SetElement semantics are provided to make this clear.
    public T this[int index]
    {
      get => Elements[Offset + index];
      set => Elements[Offset + index] = value;
    }*/

    /// <summary>
    /// The simple sum of the span offset and count to make this commonly required sum simple.
    /// </summary>
    public int OffsetPlusCount => Offset + Count;

    /// <summary>
    /// Constructs a new span ready to receive elements. All new spans a empty (ie: Count = 0)
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="offset"></param>
    /// <param name="capacity"></param>
    /// <param name="poolAllocated"></param>
    public TRexSpan(T[] elements, int offset, int capacity, bool poolAllocated = true)
    {
      Elements = elements;
      Offset = offset;
      Count = 0;
      Capacity = capacity;
      PoolAllocated = poolAllocated;
    }

    /// <summary>
    /// Inserts a provided element into the nominated index
    /// </summary>
    /// <param name="element"></param>
    /// <param name="index"></param>
    public void Insert(T element, int index)
    {
      if (index < 0 || index >= Count)
      {
        throw new ArgumentException("Index out of range");
      }

      Array.Copy(Elements, Offset + index, Elements, index + 1, Count - index);
      Elements[Offset + index] = element;
      Count++;
    }

    /// <summary>
    /// Sets the given element into the index'th element in the list of elements in this span
    /// </summary>
    /// <param name="element"></param>
    /// <param name="index"></param>
    public void SetElement(T element, int index)
    {
      if (index < 0 || index >= Count)
      {
        throw new ArgumentException("Index out of range");
      }

      Elements[Offset + index] = element;
    }

    /// <summary>
    /// Retrieves the index'th element from the list of elements in this span.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T GetElement(int index)
    {
      if (index < 0 || index >= Count)
      {
        throw new ArgumentException("Index out of range");
      }

      return Elements[Offset + index];
    }

    /// <summary>
    /// Adds a new element to the span. If there is no spare capacity available an argument exception will be thrown
    /// </summary>
    /// <param name="element"></param>
    public void Add(T element)
    {
      if (Count >= Capacity)
      {
        throw new ArgumentException($"No spare capacity to add new element, capacity = {Capacity}, element count = {Count}");
      }

      Elements[Offset + Count++] = element;
    }

    /// <summary>
    /// Returns the first element in the slab
    /// </summary>
    /// <returns></returns>
    public T First() => Elements[Offset];

    /// <summary>
    /// Returns the last element in the slab
    /// </summary>
    /// <returns></returns>
    public T Last() => Elements[Offset + Count - 1];

    /// <summary>
    /// Copies a number of elements from the start of the source span to the start of the target span
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceCount"></param>
    public void Copy(TRexSpan<T> source, int sourceCount)
    {
      if (sourceCount < 0 || sourceCount > source.Count)
      {
        throw new ArgumentException("Source count may not be negative or greater than the count of elements in the source");
      }

      if (Capacity < sourceCount)
      {
        throw new ArgumentException($"Target has insufficient capacity ({Capacity}) to contain required items from source ({sourceCount})");
      }

      Array.Copy(source.Elements, source.Offset, Elements, Offset, sourceCount);
      Count = Math.Max(Count, sourceCount);
    }
  }
}
