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

    public const int MAX_ALLOCATION_POOL_SIZE = 2 * 65536;

    private readonly int _allocationPoolPageSize;

    /// <summary>
    /// A singleton empty buffer
    /// </summary>
    private static readonly TRexSpan<T> ZeroBuffer = new TRexSpan<T>(new T[0], TRexSpan<T>.NO_SLAB_INDEX, 0, 0, false);

    /// <summary>
    /// The set of pools providing arrays of different sizes
    /// </summary>
    private readonly SlabAllocatedPool<T>[] _pools;

    public SlabAllocatedArrayPool(int allocationPoolPageSize = MAX_ALLOCATION_POOL_SIZE)
    {
      _allocationPoolPageSize = allocationPoolPageSize;

      if (allocationPoolPageSize < 1 || allocationPoolPageSize > MAX_ALLOCATION_POOL_SIZE)
      {
        throw new ArgumentException($"Allocation pool size must be in the range 1..{MAX_ALLOCATION_POOL_SIZE}");
      }

      if (1 << (Utilities.Log2(allocationPoolPageSize) - 1) != allocationPoolPageSize)
      {
        throw new ArgumentException("Allocation pool page size must be a power of 2");
      }

      _pools = new SlabAllocatedPool<T>[Utilities.Log2(allocationPoolPageSize)];
      for (int i = 0, limit = _pools.Length; i < limit; i++)
      {
          _pools[i] = new SlabAllocatedPool<T>(allocationPoolPageSize, 1 << i);
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

      var log2 = Utilities.Log2(minSize) - 1;
      if (minSize > 1 << log2)
      {
        log2++;
      }

      if (log2 >= _pools.Length)
      {
        // Requested buffer is too large. Note the request in the log and return a buffer of the requested size
        Log.LogInformation($"Array pool serviced request for buffer of {minSize} elements, above the maximum of {_allocationPoolPageSize}");
        return new TRexSpan<T>(new T[minSize], TRexSpan<T>.NO_SLAB_INDEX, 0, minSize, false);
      }

      return _pools[log2].Rent();
    }

    /// <summary>
    /// Returns a given buffer back to the allocation pool
    /// </summary>
    /// <param name="buffer"></param>
    public void Return(ref TRexSpan<T> buffer)
    {
      if (!buffer.NeedsToBeReturned())
      {
        return;
      }

      if (buffer.Capacity == 0)
      {
        // This is either a default initialized span, or the zero element. In both cases just ignore it
        return;
      }

#if CELLDEBUG
      if (buffer.IsReturned)
      {
        throw new ArgumentException($"Buffer being return is not on rental: Offset = {buffer.Offset}, Count = {buffer.Count}, Capacity = {buffer.Capacity}");
      }
#endif

      // Find the appropriate pool and return an element from it
      var log2 = Utilities.Log2(buffer.Capacity) - 1;

      // Return the span to the pool if it is not the zero element
      _pools[log2].Return(ref buffer);
    }

    /// <summary>
    /// Clones the content 'oldBuffer' by creating a new TRexSpan and copying the elements from oldBuffer into it
    /// </summary>
    /// <param name="oldBuffer"></param>
    /// <returns></returns>
    public TRexSpan<T> Clone(TRexSpan<T> oldBuffer)
    {
#if CELLDEBUG
      if (oldBuffer.IsReturned)
      {
        throw new ArgumentException($"Buffer being cloned is not on rental: Offset = {oldBuffer.Offset}, Count = {oldBuffer.Count}, Capacity = {oldBuffer.Capacity}");
      }
#endif

      // Get a new buffer
      var newBuffer = Rent(oldBuffer.Count);
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
    public (int poolIndex, int arraySize, int capacity, int rentedItems)[] Statistics()
    {
      var result = new (int poolIndex, int arraySize, int capacity, int availableItems)[_pools.Length];

      for (int i = 0, limit = _pools.Length; i < limit; i++)
      {
         result[i] = (i, _pools[i].ArraySize, _pools[i].Capacity, _pools[i].RentalTideLevel + 1);
      }

      return result;
    }
  }
}
