using System;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.IO
{
  public class GenericTwoDArrayCache<T> : IGenericTwoDArrayCache<T>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericTwoDArrayCache<T>>();

    private readonly int _dimX;
    private readonly int _dimY;

    private readonly T[][,] _cache;
    private int _cacheCount;
    private readonly int _maxCacheSize;
    private readonly int _maxCacheSizeMinus1;

    private int _currentWaterMark;
    private int _highWaterMark;

    public GenericTwoDArrayCache(int dimX, int dimY, int maxCacheSize)
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
        _currentWaterMark++;
        if (_currentWaterMark > _highWaterMark)
        {
          _highWaterMark = _currentWaterMark;
        }

        if (_cacheCount > 0)
        {
          return _cache[--_cacheCount];
        }
      }

      // Log.LogInformation($"Created new rental item for 2D cache [of {typeof(T).Name}].");

      return new T[_dimX, _dimY];
    }

    public T[,] RentEx(Action<T[,]> validator)
    {
      var rental = Rent();
      validator?.Invoke(rental);

      return rental;
    }

    public void Return(ref T[,] value)
    {
      var valueToReturn = value;
      value = null;

      lock (_cache)
      {
        _currentWaterMark--;
        if (_cacheCount <= _maxCacheSizeMinus1)
        {
          _cache[_cacheCount++] = valueToReturn;
          return;
        }
      }

      // Drop the value on the floor and log this action
      Log.LogInformation($"Returned item for 2D cache [of {typeof(T).Name}] dropped as cache is full with {_cacheCount} items [max = {_maxCacheSize}].");
    }

    public (int currentSize, int maxSize, int currentWaterMark, int highWaterMark) Statistics()
    {
      lock (_cache)
      {
        return (_cacheCount, _maxCacheSize, _currentWaterMark, _highWaterMark);
      }
    }

    private readonly string _typeName = typeof(T).Name;

    public string TypeName() => _typeName;

    public void Clear()
    {
      lock (_cache)
      {
        for (int i = 0; i < _cacheCount; i++)
          _cache[i] = null;

        _cacheCount = 0;
      }
    }
  }
}
