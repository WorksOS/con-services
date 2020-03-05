using System;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Facility within TRex to provided shared buffer pools of byte arrays across TRex operations
  /// It defines a set of buffer pools in exponentially increasing sizes up to 1Mb in size
  /// </summary>
  public class GenericArrayPoolCaches<T> : IGenericArrayPoolCaches<T>
  {
    private readonly object _lock = new object();

    public static readonly int[] DEFAULT_POOL_CACHE_SIZES =
    {
      1, // Satisfied by the zero buffer item
      200, // 1
      200, // 2
      400, // 4 - This size experiences significant overflows reported in the logs, increased to 400 to compensate
      400, // 8 - This size experiences significant overflows reported in the logs, increased to 400 to compensate
      800, // 16 - This size experiences significant overflows reported in the logs, increased to 800 to compensate
      200, // 32
      200, // 64
      400, // 128 - This size experiences significant overflows reported in the logs, increased from 200 to 400 to compensate
      200, // 256
      200, // 512
      200, // 1024
      20, // 2048
      20, // 4096
      20, // 8192
      20, // 16K
      20, // 64K
      20, // 128K
      20, // 256K
      10, // 512K
      10, // 1024K
    };

    public const int MAX_BUFFER_SIZE_CACHED = 1 << NumExponentialPoolsToProvide; // ~1 million items

    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericArrayPoolCaches<T>>();

    /// <summary>
    /// The number of different power-of-2 sized buffer pools to rent buffers from
    /// </summary>
    private const int NumExponentialPoolsToProvide = 20; // Up to ~1 million items

    /// <summary>
    /// A singleton empty buffer
    /// </summary>
    private static readonly T[] ZeroBuffer = new T[0];

    /// <summary>
    /// The collection of pools individual buffers are rented out from
    /// </summary>
    private readonly T[][][] _pools;

    private readonly int _poolsLength;

    /// <summary>
    /// Counters for each of the power-of-two buffer pools
    /// </summary>
    private readonly GenericArrayPoolStatistics[] _poolCounts = new GenericArrayPoolStatistics[NumExponentialPoolsToProvide + 1];

    public GenericArrayPoolCaches()
    {
      _pools = new T[NumExponentialPoolsToProvide + 1][][];
      _poolsLength = _pools.Length;

      // Establish rentable buffers per the default sizing
      for (var i = 0; i < _pools.Length; i++)
      {
        _pools[i] = new T[DEFAULT_POOL_CACHE_SIZES[i]][];
      }

      for (var i = 0; i < _poolsLength; i++)
      {
        _poolCounts[i] = new GenericArrayPoolStatistics
        {
          PoolIndex = i,
          PoolCapacity = _pools[i].Length,
          AvailCount = 0 // Pools don't start pre-populated with objects
        };
      }
    }

    /// <summary>
    /// Rents out a buffer from the pool. The buffer is greater than or equal to the size of the requested buffer.
    /// Pooled buffers are created on demand - each pool is not initialized with a full set of buffers.
    /// Note: The pool does not maintain a reference to the buffer. If tha renter fails to return the buffer pool
    /// it will be cleaned up by the garbage collector.
    /// </summary>
    /// <param name="minSize"></param>
    /// <returns></returns>
    public T[] Rent(int minSize)
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

      if (log2 >= _poolsLength)
      {
        // Requested buffer is too large. Note the request in the log and return a buffer of the requested size
        Log.LogInformation($"Elements buffer pool serviced request for buffer of {minSize} bytes, above the maximum of {1 << (NumExponentialPoolsToProvide - 1)}");
        return new T[minSize];
      }

      lock (_lock)
      {
        var newCurrentRent = ++_poolCounts[log2].CurrentRents;

        if (newCurrentRent > _poolCounts[log2].HighWaterRents)
          _poolCounts[log2].HighWaterRents = newCurrentRent;

        if (_poolCounts[log2].AvailCount > 0)
        {
          var pool = _pools[log2];
          var buffer = pool[--_poolCounts[log2].AvailCount];

          pool[_poolCounts[log2].AvailCount] = null;

          return buffer;
        }

        // No rent able elements, so create an appropriate sized buffer for the rental
        //  Log.LogInformation($"Memory buffer pool serviced request for buffer of {minSize} bytes, but the appropriate pool has ");
        return new T[1 << log2];
      }
    }
    
    /// <summary>
    /// Returns a previously rented buffer from the pool.
    /// Note: If the size of the returned buffer is not a power of two it will be logged but otherwise ignored.
    /// </summary>
    /// <param name="buffer"></param>
    public void Return(ref T[] buffer)
    {
      var bufferLength = buffer.Length;

      // Find the appropriate pool and ensure it is the correct size. If not, just ignore it
      var log2 = Utilities.Log2(bufferLength) - 1;
      if (bufferLength != 1 << log2)
      {
        // Don't complain bout return of the zero'th element
        if (bufferLength > 0)
        {
          Log.LogWarning($"Elements buffer pool returned buffer not power-of-two in size: {bufferLength}. Ignoring this returned buffer");
        }

        buffer = null;
        return;
      }

      if (log2 >= NumExponentialPoolsToProvide)
      {
        // Buffer is too big to place back into the cache and note it in log
        Log.LogWarning($"Elements buffer pool returned buffer too big [{bufferLength}] for an existing cache pool. Ignoring this returned buffer");
        buffer = null;
        return;
      }

      lock (_lock)
      {
        --_poolCounts[log2].CurrentRents;

        if (_poolCounts[log2].AvailCount >= _poolCounts[log2].PoolCapacity)
        {
          // The pool is full - cut this buffer loose for the GC to look after
          Log.LogWarning($"Elements buffer pool full (size={_poolCounts[log2].PoolCapacity}) for {typeof(T).Name}[] buffers of size {bufferLength}. Ignoring this returned buffer");
        }
        else
        {
          // Place the returned buffer into the pool for later reuse
          _pools[log2][_poolCounts[log2].AvailCount++] = buffer;
        }

        buffer = null;
      }
    }

    /// <summary>
    /// Supplies statistics on the usage of the cached array pools
    /// </summary>
    /// <returns></returns>
    public GenericArrayPoolStatistics[] Statistics()
    {
      lock (_poolCounts)
      {
        var result = new GenericArrayPoolStatistics[_poolCounts.Length];
        Array.Copy(_poolCounts, result, _poolCounts.Length);

        return result;
      }
    }

    private static readonly string _typeName = typeof(T).Name;
    public string TypeName() => _typeName;

    /// <summary>
    /// Clears any content in the array pool and reset all availability counters to zero
    /// </summary>
    public void Clear()
    {
      lock (_pools)
      {
        for (var i = 0; i < _poolsLength; i++)
        {
          for (var j = 0; j < _poolCounts[i].PoolCapacity; j++)
          {
            _pools[i][j] = null;
          }

          _poolCounts[i].AvailCount = 0;
        }
      }
    }
  }
}
