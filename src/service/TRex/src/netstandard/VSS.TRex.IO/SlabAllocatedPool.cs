using System;
using Microsoft.Extensions.Logging;

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

    public const int MAXIMUM_PROVISIONED_SLAB_COUNT = 250;

    public readonly int PoolSize;
    public readonly int ArraySize;

    private SlabAllocatedPoolPage<T>[] _slabPages;

    private int _rentalTideLevel;
    public int RentalTideLevel => _rentalTideLevel;

    private int _capacity;
    public int Capacity => _capacity;

    private readonly int _maxCapacity;

    public readonly int SpanCountPerSlabPage;

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
      lock (this)
      {
        if (_rentalTideLevel >= _maxCapacity)
        {
          // The pool is empty. Synthesize a new span and return it. This span will be discarded when returned
          Log.LogInformation($"Array pool for spans of size {ArraySize} has reached max capacity - returning non pool allocated span");

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

        #if CELLDEBUG
        var buffer = _slabPages[slabIndex].Arrays[slabOffset];

        if (!buffer.IsReturned)
        {
          throw new ArgumentException($"Buffer is not returned to pool on re-rental: Offset = {buffer.Offset}, Count = {buffer.Count}, Capacity = {buffer.Capacity}");
        }

        buffer.IsReturned = false;
        #endif

        return _slabPages[slabIndex].Arrays[slabOffset];
      }
    }

    /// <summary>
    /// Returns a given span buffer back to the allocation pool
    /// </summary>
    /// <param name="buffer"></param>
    public void Return(TRexSpan<T> buffer)
    {
#if CELLDEBUG
      if (buffer.Elements != _slabPages[buffer.SlabIndex] || buffer.Capacity != ArraySize)
      {
        throw new ArgumentException("Buffer span being returned to a pool that did not create it");
      }
#endif

      lock (this)
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

        // Adjust the buffer slab index to match the one it is being returned to.
        // This means the slab containing the span metadata and the slab containing the 
        // span elements may validly be different.
        buffer.SlabIndex = (byte)(_rentalTideLevel / SpanCountPerSlabPage);
        _slabPages[buffer.SlabIndex].Arrays[_rentalTideLevel % SpanCountPerSlabPage] = buffer;
        _rentalTideLevel--;
      }
    }
  }
}
