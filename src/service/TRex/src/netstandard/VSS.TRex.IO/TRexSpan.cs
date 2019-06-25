using System;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Defines a span of elements within a larger array of elements
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public struct TRexSpan<T>
  {
    /// <summary>
    /// This identifies a span that does not exist in a pool allocated slab
    /// </summary>
    public const int NO_SLAB_INDEX = int.MaxValue;

#if CELLDEBUG
    // Debugging flag
    public bool IsReturned;
#endif

    /// <summary>
    /// The index of the slab in the pool that currently contains this Span description.
    /// Note: This slab may not be the one that contains the elements this span describes,
    /// the Elements property is the canonical reference for this relationship
    /// </summary>
    private readonly int SlabIndex;

    /// <summary>
    /// The offset within the Elements array of this slab in the pool that the first element in the span occurs in
    /// </summary>
    public readonly int Offset;

    /// <summary>
    /// The number of defined elements present in the space Count is less than or equal to Length
    /// </summary>
    public int Count;

    /// <summary>
    /// The capacity of the span
    /// </summary>
    public readonly int Capacity;

    private T[] _elements;

    /// <summary>
    /// The reference to the slab allocated array that this span's elements are contained
    /// </summary>
    public T[] Elements
    {
      get => _elements;
    }

    /* Note: TR deliberately does not provide an indexer as it can contain structs which causes issues with
  return of copies of the struct. GetElement/SetElement semantics are provided to make this clear.
    public T this[int index]
    {
      get => _elements[Offset + index];
      set => _elements[Offset + index] = value;
    }*/

    /// <summary>
    /// The simple sum of the span offset and count to make this commonly required sum simple.
    /// </summary>
    public int OffsetPlusCount => Offset + Count;

    /// <summary>
    /// Constructs a new span ready to receive elements. All new spans a empty (ie: Count = 0)
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="slabIndex"></param>
    /// <param name="offset"></param>
    /// <param name="capacity"></param>
    public TRexSpan(T[] elements, int slabIndex, int offset, int capacity, bool isReturned)
    {
      _elements = elements;
      Offset = offset;
      SlabIndex = slabIndex;
      Count = 0;
      Capacity = capacity;

#if CELLDEBUG
       IsReturned = isReturned;
#endif
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

      Array.Copy(_elements, Offset + index, _elements, Offset + index + 1, Count - index);
      _elements[Offset + index] = element;
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

      _elements[Offset + index] = element;
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

      return _elements[Offset + index];
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

      _elements[Offset + Count] = element;
      Count++;
    }

    /// <summary>
    /// Returns the first element in the slab
    /// </summary>
    /// <returns></returns>
    public T First() => _elements[Offset];

    /// <summary>
    /// Returns the last element in the slab
    /// </summary>
    /// <returns></returns>
    public T Last() => _elements[Offset + Count - 1];

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

      Array.Copy(source.Elements, source.Offset, _elements, Offset, sourceCount);
      Count = source.Count;
    }

    /// <summary>
    /// Copies a number of elements from the start of the source array to the start of the target span
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceCount"></param>
    public void Copy(T[] source, int sourceCount)
    {
      if (sourceCount < 0 || sourceCount > source.Length)
      {
        throw new ArgumentException("Source count may not be negative or greater than the count of elements in the source");
      }

      if (Capacity < sourceCount)
      {
        throw new ArgumentException($"Target has insufficient capacity ({Capacity}) to contain required items from source ({sourceCount})");
      }

      Array.Copy(source, 0, _elements, Offset, sourceCount);
      Count = Math.Max(Count, sourceCount);
    }

    /// <summary>
    /// Indicates if this span should be returned to the pool at a future point in time
    /// </summary>
    /// <returns></returns>
    public bool NeedsToBeReturned() => Capacity > 0 && SlabIndex != NO_SLAB_INDEX && _elements != null;

    public void MarkReturned()
    {
      _elements = null;
    }
  }
}
