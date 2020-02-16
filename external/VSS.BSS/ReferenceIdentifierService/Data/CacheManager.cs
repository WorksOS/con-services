using System;
using System.Runtime.Caching;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using System.Linq;

namespace VSS.Nighthawk.ReferenceIdentifierService.Data
{
  public class CacheManager: ICacheManager
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
      if (null != value)
      {
        _memoryCache.Set(key, value, policy:
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
      if (cacheItem != null)
        return cacheItem.Value;
      else
        return null;
    }

    /// <summary>
    /// assumes that if key starts with the cache item and the is the longest matching string in the cache then it is the closest match
    /// </summary>
    /// <param name="key">string to find the closest match on for now just urls are being used</param>
    /// <returns></returns>
    public object GetClosestData(string key)
    {
      var cacheItem = (from m in _memoryCache.AsEnumerable()
                       where key.ToLower().StartsWith(m.Key.ToLower())
                       orderby m.Key.Length descending
                       select m.Value).ToList();

      return cacheItem.FirstOrDefault();
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
