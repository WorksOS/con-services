using System;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Implements a pool of small arrays that are allocated together into a single allocated slab
  /// and represented by TRexSpan instances
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SlabAllocatedPool<T>
  {
    public readonly int PoolSize;
    public readonly int ArraySize;
    public readonly int SpanCount;

    private readonly T[] _slab;
    private readonly TRexSpan<T>[] _arrays;

    private int _availCount;
    public int AvailCount => _availCount;

    /// <summary>
    /// Creates a new pool of sub-arrays of arraySize size within an overall slab allocated array
    /// of poolSize size.
    /// Both poolSize and arraySize are required to be a power of two for optimal use of allocated memory
    /// </summary>
    /// <param name="poolSize"></param>
    /// <param name="arraySize"></param>
    public SlabAllocatedPool(int poolSize, int arraySize)
    {
      if (1 << (Utilities.Log2(poolSize) - 1) != poolSize)
      {
        throw new ArgumentException($"Pool size of {poolSize} is not a power of two as required.");
      }

      if (1 << (Utilities.Log2(arraySize) - 1) != arraySize)
      {
        throw new ArgumentException($"Array size of {arraySize} is not a power of two as required.");
      }

      PoolSize = poolSize;
      ArraySize = arraySize;

      // Create a single allocation to contain a slab of elements of size pool size
      _slab = new T[PoolSize];

      SpanCount = PoolSize / ArraySize;
      _availCount = SpanCount;

      // Create an array of sub array spans that fit within the overall slab
      _arrays = new TRexSpan<T>[_availCount];
      for (int i = 0, limit = _arrays.Length; i < limit; i++)
      {
        _arrays[i] = new TRexSpan<T>(_slab, i * ArraySize, ArraySize, true, true);
      }
    }

    public TRexSpan<T> Rent()
    {
      lock (_arrays)
      {
        if (_availCount == 0)
        {
          // The pool is empty. Synthesize a new span and return it. This span will be discarded when returned
          return new TRexSpan<T>(new T[ArraySize], 0, ArraySize, false, false);
        }

        var buffer = _arrays[--_availCount];

        if (!buffer.IsReturned)
        {
          throw new ArgumentException($"Buffer is not returned to pool on re-rental: Offset = {buffer.Offset}, Count = {buffer.Count}, Capacity = {buffer.Capacity}");
        }

        buffer.IsReturned = false;

        return buffer;
      }
    }

    public void Return(TRexSpan<T> buffer)
    {
      if (buffer.Elements != _slab || buffer.Capacity != ArraySize)
      {
        throw new ArgumentException("Buffer span being returned to a pool that did not create it");
      }

      lock (_arrays)
      {
        buffer.Count = 0;
        buffer.IsReturned = true;
        _arrays[_availCount++] = buffer;
      }
    }
  }
}
