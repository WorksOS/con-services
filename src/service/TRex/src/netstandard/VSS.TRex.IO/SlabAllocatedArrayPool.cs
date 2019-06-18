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
    public const int DefaultNumExponentialPoolsToProvide = 20; // 1,000,000 elements

    public const int MaxNumExponentialPoolsToProvide = 23; // 8 million elements

    private readonly int _numExponentialPoolsToProvide;

    /// <summary>
    /// A singleton empty buffer
    /// </summary>
    private static readonly TRexSpan<T> ZeroBuffer = new TRexSpan<T>(new T[0], 0, 0, false);

    /// <summary>
    /// The set of pools providing arrays of different sizes
    /// </summary>
    private readonly SlabAllocatedPool<T>[] _pools;

    public SlabAllocatedArrayPool(int elementsPerPool, int numExponentialPoolsToProvide = -1)
    {
      _numExponentialPoolsToProvide = numExponentialPoolsToProvide == -1
        ? DefaultNumExponentialPoolsToProvide
        : numExponentialPoolsToProvide;

      if (numExponentialPoolsToProvide > MaxNumExponentialPoolsToProvide)
      {
        throw new ArgumentException($"Cannot create slab allocated array pool with more than {1 << MaxNumExponentialPoolsToProvide} elements per pool");
      }

      _pools = new SlabAllocatedPool<T>[_numExponentialPoolsToProvide];
      for (int i = 0, limit = _pools.Length; i < limit; i++)
      {
        _pools[i] = new SlabAllocatedPool<T>(elementsPerPool, 1 << i);
      }
    }

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

      var log2 = Utilities.Log2(minSize) - 1;
      if (minSize > 1 << log2)
      {
        log2++;
      }

      if (log2 >= _pools.Length)
      {
        // Requested buffer is too large. Note the request in the log and return a buffer of the requested size
        Log.LogInformation(
          $"Array pool serviced request for buffer of {minSize} elements, above the maximum of {1 << _numExponentialPoolsToProvide}");
        return new TRexSpan<T>(new T[minSize], 0, minSize, false);
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

      // Find the appropriate pool and return an element from it
      var log2 = Utilities.Log2(buffer.Capacity) - 1;

      // Return the span to the pool if it is not the zero element
      _pools[log2].Return(buffer);
    }

    public TRexSpan<T> Clone(TRexSpan<T> oldBuffer)
    {
      // Get a new buffer
      var newBuffer = Rent(oldBuffer.Capacity);
      newBuffer.Count = oldBuffer.Count;

      // Copy elements from the old buffer to the new buffer
      Array.Copy(oldBuffer.Elements, oldBuffer.Offset, newBuffer.Elements, newBuffer.Offset, oldBuffer.Capacity);

      // ... and return the newly resized result
      return newBuffer;
    }
  }
}
