using System;
using System.Runtime.Caching;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Data
{
  public class CacheManager : ICacheManager
  {
    private readonly ObjectCache _memoryCache;

    public CacheManager(ICacheManagerConfig cacheManagerConfig)
    {
      if (string.IsNullOrEmpty(cacheManagerConfig.CacheManagerName))
      {
        throw new ArgumentException("StringNullOrEmpty", "cacheManagerName");
      }
      _memoryCache = cacheManagerConfig.Config != null ? 
        new MemoryCache(cacheManagerConfig.CacheManagerName, cacheManagerConfig.Config) : 
        new MemoryCache(cacheManagerConfig.CacheManagerName);
    }

    public void Add(string key, object value, int cacheLifetimeMinutes)
    {
      if(null != value)
      {
        _memoryCache.Add(key, value, policy:
                                       new CacheItemPolicy
                                         {
                                           Priority = CacheItemPriority.Default,
                                           AbsoluteExpiration = AbsoluteTime(TimeSpan.FromMinutes(cacheLifetimeMinutes))
                                         });
      }
    }

    public bool Contains(string key)
    {
      return _memoryCache.Contains(key);
    }

    public object GetData(string key)
    {
      var cacheItem = _memoryCache.GetCacheItem(key);
      if (cacheItem != null) return cacheItem.Value;
      return null;
    }

    public void Remove(string key)
    {
      _memoryCache.Remove(key);
    }

    public DateTimeOffset AbsoluteTime(DateTime absoluteTime)
    {
      if (!(absoluteTime > DateTime.Now))
        throw new ArgumentOutOfRangeException("absoluteTime");

      return absoluteTime.ToUniversalTime();
    }

    public DateTimeOffset AbsoluteTime(TimeSpan timeFromNow)
    {
      return AbsoluteTime(DateTime.Now + timeFromNow);
    }

  }
}
