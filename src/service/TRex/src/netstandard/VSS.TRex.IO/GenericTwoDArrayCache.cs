using Microsoft.Extensions.Logging;

namespace VSS.TRex.IO
{
  public class TwoDArrayCache<T> : ITwoDArrayCache<T>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TwoDArrayCache<T>>();

    private readonly int _dimX;
    private readonly int _dimY;

    private readonly T[][,] _cache;
    private int _cacheCount;
    private readonly int _maxCacheSize;
    private readonly int _maxCacheSizeMinus1;

    public TwoDArrayCache(int dimX, int dimY, int maxCacheSize)
    {
      _dimX = dimX;
      _dimY = dimY;
      _maxCacheSize = maxCacheSize;
      _maxCacheSizeMinus1 = maxCacheSize - 1;

      _cache = new T[maxCacheSize][,];
      _cacheCount = 0;
    }

    public T[,] Rent()
    {
      lock (_cache)
      {
        if (_cacheCount > 0)
        {
          return _cache[--_cacheCount];
        }
      }

      Log.LogInformation($"Created new rental item for 2D cache [of {typeof(T).Name}].");

      return new T[_dimX, _dimY];
    }

    public void Return(T[,] value)
    {
      lock (_cache)
      {
        if (_cacheCount < _maxCacheSizeMinus1)
        {
          _cache[_cacheCount++] = value;
          return;
        }
      }

      // Drop the value on the floor and log this action
      Log.LogInformation($"Returned item for 2D cache [of {typeof(T).Name}] dropped as cache is full with {_cacheCount} items [max = {_maxCacheSize}].");
    }

    public (int currentSize, int maxSize) Statistics()
    {
      return (_cacheCount, _maxCacheSize);
    }
  }
}
