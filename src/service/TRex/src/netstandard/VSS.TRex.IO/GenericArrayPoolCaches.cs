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
    /// <summary>
    /// For arrays with sizes up to and including 1024 elements, this is the number of slots to
    /// provide for each size bucket (1, 2, 4, 16, 32, 64, 126, 256, 512 & 1024)
    /// </summary>
    public const int SMALL_POOL_CACHE_SIZE = 100;

    /// <summary>
    /// For arrays with sizes above 1024 items, and less than 512k elements, this is the number of slots to
    /// provide for each size bucket (2048, 4096, 8192, 16384, 65536, 128K, 256K)
    /// </summary>
    public const int MEDIUM_POOL_CACHE_SIZE = 20;
    
    /// <summary>
    /// For arrays with sizes above 256k items this is the number of slots to
    /// provide for each size bucket (512K, 1024K only)
    /// </summary>
    public const int LARGE_POOL_CACHE_SIZE = 10;

    public const int MAX_BUFFER_SIZE_CACHED = 1 << 20; // ~1 million items

    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericArrayPoolCaches<T>>();

    /// <summary>
    /// The number of different power-of-2 sized buffer pools to rent buffers from
    /// </summary>
    private const int NumExponentialPoolsToProvide = 21;

    /// <summary>
    /// A singleton empty buffer
    /// </summary>
    private static readonly T[] ZeroBuffer = new T[0];

    /// <summary>
    /// The collection of pools individual buffers are rented out from
    /// </summary>
    private readonly T[][][] _pools;
    
    /// <summary>
    /// Counters for each of the power-of-two buffer pools
    /// </summary>
    private readonly int[] _poolCounts = new int[NumExponentialPoolsToProvide];

    public GenericArrayPoolCaches()
    {
      _pools = new T[NumExponentialPoolsToProvide][][];

      // Establish 100 small rent able buffers for anything up to 1024 items
      for (int i = 0; i < 10; i++)
      {
        _pools[i] = new T[SMALL_POOL_CACHE_SIZE][];
      }

      // Establish 20 small rent able buffers for anything up to 256K items

      for (int i = 10; i < 19; i++)
      {
        _pools[i] = new T[MEDIUM_POOL_CACHE_SIZE][];
      }

      // Establish 10 512K and 1M item buffers
      _pools[19] = new T[LARGE_POOL_CACHE_SIZE][];
      _pools[20] = new T[LARGE_POOL_CACHE_SIZE][];
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

      if (log2 >= _pools.Length)
      {
        // Requested buffer is too large. Note the request in the log and return a buffer of the requested size
        Log.LogInformation($"Elements buffer pool serviced request for buffer of {minSize} bytes, above the maximum of {1 << NumExponentialPoolsToProvide}");
        return new T[minSize];
      }

      lock (_poolCounts)
      {
        if (_poolCounts[log2] > 0)
        {
          var pool = _pools[log2];
          var buffer = pool[--_poolCounts[log2]];

          pool[_poolCounts[log2]] = null;

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
    public void Return(T[] buffer)
    {
      // Find the appropriate pool and ensure it is the correct size. If not, just ignore it
      var log2 = Utilities.Log2(buffer.Length) - 1;
      if (buffer.Length != 1 << log2)
      {
        Log.LogWarning($"Elements buffer pool returned buffer not power-of-two in size: {buffer.Length}. Ignoring this returned buffer");
        return;
      }

      if (log2 >= NumExponentialPoolsToProvide)
      {
        // Buffer is too big to place back into the cache and note it in log
        Log.LogWarning($"Elements buffer pool returned buffer too big [{buffer.Length}] for an existing cache pool. Ignoring this returned buffer");
        return;
      }

      lock (_poolCounts)
      {
        if (_poolCounts[log2] >= _pools[log2].Length)
        {
          // The pool is full - cut this buffer loose for the GC to look after
          Log.LogWarning($"Elements buffer pool full for buffers of size {buffer.Length}. Ignoring this returned buffer");
          return;
        }
        
        // Place the returned buffer into the pool for later reuse
        _pools[log2][_poolCounts[log2]++] = buffer;
      }
    }
  }
}
