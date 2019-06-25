using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Implements a pool of small arrays that are allocated together into a larger allocated slabs
  /// and represented by TRexSpan instances
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SlabAllocatedPool<T>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SlabAllocatedPool<T>>();

    public const int MAXIMUM_PROVISIONED_SLAB_COUNT = 1000;

    public readonly int PoolSize;
    public readonly int ArraySize;

    private SlabAllocatedPoolPage<T>[] _slabPages;

    private int _rentalTideLevel;
    public int RentalTideLevel => _rentalTideLevel;

    private int _capacity;
    public int Capacity => _capacity;

    private readonly int _maxCapacity;

    public readonly int SpanCountPerSlabPage;

    private readonly object _slabPagesLock = new object();

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
      SpanCountPerSlabPage = PoolSize / arraySize;

      // Create a single allocation to contain a slab of elements of size pool size
      _slabPages = new SlabAllocatedPoolPage<T>[0];

      _rentalTideLevel = -1;
      _maxCapacity = MAXIMUM_PROVISIONED_SLAB_COUNT * SpanCountPerSlabPage;
    }

    public TRexSpan<T> Rent()
    {
      lock (_slabPagesLock)
      {
        if (_rentalTideLevel >= _maxCapacity)
        {
          // The pool is empty. Synthesize a new span and return it. This span will be discarded when returned
          Log.LogInformation($"Array pool for spans of size {ArraySize} has reached max capacity of {_maxCapacity} - returning non pool allocated span");

          return new TRexSpan<T>(new T[ArraySize], TRexSpan<T>.NO_SLAB_INDEX, 0, ArraySize, false);
        }

        _rentalTideLevel++;
        var slabIndex = _rentalTideLevel / SpanCountPerSlabPage;
        var slabOffset = _rentalTideLevel % SpanCountPerSlabPage;

        if (slabIndex >= _slabPages.Length)
        { 
          // Need to provision a new slab. This is no optimal code from a performance perspective,
          // but it will happen very rarely in level flight operations.
          Array.Resize(ref _slabPages, _slabPages.Length + 1);
          _slabPages[_slabPages.Length - 1] = new SlabAllocatedPoolPage<T>(PoolSize, ArraySize);
          _capacity = _slabPages.Length * SpanCountPerSlabPage;
        }

        var buffer = _slabPages[slabIndex].Arrays[slabOffset];

        #if CELLDEBUG
        if (!buffer.IsReturned)
        {
          throw new ArgumentException($"Buffer is not returned to pool on re-rental: Offset = {buffer.Offset}, Count = {buffer.Count}, Capacity = {buffer.Capacity}");
        }
        buffer.IsReturned = false;
        #endif

        if (buffer.Count != 0)
        {
          throw new TRexException("Rented buffer count is not zero");
        }

        return buffer;
      }
    }

    /// <summary>
    /// Returns a given span buffer back to the allocation pool
    /// </summary>
    /// <param name="buffer"></param>
    public void Return(TRexSpan<T> buffer)
    {
      lock (_slabPagesLock)
      {
        if (_rentalTideLevel < 0)
        {
          // There is no more capacity to accept returns
          return;
        }

        buffer.Count = 0;

#if CELLDEBUG
        buffer.IsReturned = true;
#endif

        // Note, buffer slab index in the span may not match the one it is being returned to, which is OK...
        _slabPages[_rentalTideLevel / SpanCountPerSlabPage].Arrays[_rentalTideLevel % SpanCountPerSlabPage] = buffer;
        _rentalTideLevel--;
      }
    }
  }
}
