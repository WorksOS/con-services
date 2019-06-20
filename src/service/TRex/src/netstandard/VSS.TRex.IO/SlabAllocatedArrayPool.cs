using System;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.IO
{
  /// <summary>
  /// This class acts as a pool of arrays that it allocates from a larger slab allocations that it manages
  /// The pool supports a range of different sizes with each size being double the size of the smaller size
  /// </summary>
  public class SlabAllocatedArrayPool<T> : ISlabAllocatedArrayPool<T>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SlabAllocatedArrayPool<T>>();

    /// <summary>
    /// The number of different power-of-2 sized buffer pools to rent buffers from
    /// </summary>
    public const int DefaultLargestSizeExponentialPoolToProvide = 20; // 1,000,000 elements

    public const int SmallestSizeExponentialPoolToProvide = 1; // 1 elements
    public const int LargestSizeExponentialPoolToProvide = 23; // 8 million elements

    private readonly int _smallestExponentialPoolToProvide;
    private readonly int _largestExponentialPoolToProvide;
    private readonly int _numAllocatedPools;

    /// <summary>
    /// A singleton empty buffer
    /// </summary>
    private static readonly TRexSpan<T> ZeroBuffer = new TRexSpan<T>(new T[0], 0, 0, false, false);

    /// <summary>
    /// The set of pools providing arrays of different sizes
    /// </summary>
    private readonly SlabAllocatedPool<T>[] _pools;

    public SlabAllocatedArrayPool(int elementsPerPool, int smallestExponentialPoolToProvide, int largestExponentialPoolToProvide)
    {
      if (smallestExponentialPoolToProvide < SmallestSizeExponentialPoolToProvide || smallestExponentialPoolToProvide > LargestSizeExponentialPoolToProvide)
      {
        throw new ArgumentException($"Min/Max exponential pool range must be in {SmallestSizeExponentialPoolToProvide}..{LargestSizeExponentialPoolToProvide}");
      }

      if (largestExponentialPoolToProvide < SmallestSizeExponentialPoolToProvide || largestExponentialPoolToProvide > LargestSizeExponentialPoolToProvide)
      {
        throw new ArgumentException($"Min/Max exponential pool range must be in {SmallestSizeExponentialPoolToProvide}..{LargestSizeExponentialPoolToProvide}");
      }

      if (smallestExponentialPoolToProvide > largestExponentialPoolToProvide)
      {
        throw new ArgumentException($"Smallest exponential pool must be less than or equal to largest exponential pool to provide");
      }

      _smallestExponentialPoolToProvide = smallestExponentialPoolToProvide;
      _largestExponentialPoolToProvide = largestExponentialPoolToProvide;

      _pools = new SlabAllocatedPool<T>[_largestExponentialPoolToProvide + 1];
      for (int i = 0, limit = _pools.Length; i < limit; i++)
      {
        if (i >= _smallestExponentialPoolToProvide)
        {
          _numAllocatedPools++;
          _pools[i] = new SlabAllocatedPool<T>(elementsPerPool, 1 << i);
        }
      }
    }

    /// <summary>
    /// Rents a buffer from the appropriately sized pool in the collection. THe buffer will be equal to,
    /// or greater than, the requested size.
    /// </summary>
    /// <param name="minSize"></param>
    /// <returns></returns>
    public TRexSpan<T> Rent(int minSize)
    {
      if (minSize < 0)
      {
        throw new ArgumentException("Negative buffer size not permitted", nameof(minSize));
      }

      if (minSize == 0)
      {
        return ZeroBuffer;
      }

      // Select the pool to be used
      // Calculate the simple whole log2 <= minSize. Choose the buffer that is equal to or greater than minSize.

      var log2 = Math.Max(Utilities.Log2(minSize) - 1, _smallestExponentialPoolToProvide);
      if (minSize > 1 << log2)
      {
        log2++;
      }

      if (log2 >= _pools.Length)
      {
        // Requested buffer is too large. Note the request in the log and return a buffer of the requested size
        Log.LogInformation($"Array pool serviced request for buffer of {minSize} elements, above the maximum of {1 << LargestSizeExponentialPoolToProvide}");
        return new TRexSpan<T>(new T[minSize], 0, minSize, false, false);
      }

      return _pools[log2].Rent();
    }

    public void Return(TRexSpan<T> buffer)
    {
      if (!buffer.PoolAllocated)
      {
        // This span was created in response to no appropriate buffer to use. Discard it for the GC to clean up.
        return;
      }

      if (buffer.IsReturned)
      {
        throw new ArgumentException($"Buffer being return is not on rental: Offset = {buffer.Offset}, Count = {buffer.Count}, Capacity = {buffer.Capacity}");
      }

      // Find the appropriate pool and return an element from it
      var log2 = Utilities.Log2(buffer.Capacity) - 1;

      // Return the span to the pool if it is not the zero element
      _pools[log2].Return(buffer);
    }

    /// <summary>
    /// Clones the content 'oldBuffer' by creating a new TRexSPan and copying the elements from oldBuffer into it
    /// </summary>
    /// <param name="oldBuffer"></param>
    /// <returns></returns>
    public TRexSpan<T> Clone(TRexSpan<T> oldBuffer)
    {
      if (oldBuffer.IsReturned)
      {
        throw new ArgumentException($"Buffer being cloned is not on rental: Offset = {oldBuffer.Offset}, Count = {oldBuffer.Count}, Capacity = {oldBuffer.Capacity}");
      }

      // Get a new buffer
      var newBuffer = Rent(oldBuffer.Capacity);
      newBuffer.Count = oldBuffer.Count;

      // Copy elements from the old buffer to the new buffer
      Array.Copy(oldBuffer.Elements, oldBuffer.Offset, newBuffer.Elements, newBuffer.Offset, oldBuffer.Count);

      // ... and return the newly resized result
      return newBuffer;
    }

    /// <summary>
    /// Returns detailed statistics on each of the slab allocated array pools
    /// </summary>
    /// <returns></returns>
    public (int poolIndex, int arraySize, int capacity, int availableItems)[] Statistics()
    {
      var result = new (int poolIndex, int arraySize, int capacity, int availableItems)[_numAllocatedPools];
      var count = 0;

      for (int i = 0, limit = _pools.Length; i < limit; i++)
      {
        if (_pools[i] != null)
        {
          result[count++] = (i, _pools[i].ArraySize, _pools[i].SpanCount, _pools[i].AvailCount);
        }
      }

      return result;
    }
  }
}
