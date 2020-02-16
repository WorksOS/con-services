using System;
using System.Reflection;
using System.Runtime.Caching;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;

namespace VSS.Hosted.VLCommon.Services.MDM.Common
{
  public class CacheManager : ICacheManager
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly ObjectCache cache = MemoryCache.Default;
    
    public string GetCacheKey()
    {
      return StringConstants.CacheKey;
    }

    public string GetTokenFromCache(string cacheKey)
    {
      if (!string.IsNullOrWhiteSpace(cacheKey))
      {
        var cacheValue = cache.Get(cacheKey);
        return (cacheValue != null) ? cacheValue.ToString() : string.Empty;
      }
      Log.IfError("Invalid cache key");
      return string.Empty;
    }

    public void UpdateCacheItem(CacheItem cacheItem,int expiresTime)
    {
      if (string.IsNullOrWhiteSpace(cacheItem.Key) || string.IsNullOrWhiteSpace(cacheItem.Value.ToString())) 
        return;
      if (GetTokenFromCache(cacheItem.Key) != string.Empty)
      {
        cache.Remove(cacheItem.Key);
        cache.Add(cacheItem.Key, cacheItem.Value, new CacheItemPolicy
        {
          RemovedCallback = RefreshCacheUponExpiry,
          AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiresTime)
        });
      }
      else
      {
        cache.Add(cacheItem.Key, cacheItem.Value, new CacheItemPolicy
         {
          RemovedCallback = RefreshCacheUponExpiry,
          AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiresTime)
        });
      }
    }

    private void RefreshCacheUponExpiry(CacheEntryRemovedArguments arguments)
    {
      Log.IfInfo(string.Format("{0}'s cache item removed for the reason {1}", arguments.CacheItem.Key, arguments.RemovedReason)); 
    }

    public void RefreshCacheUponExpiry(CacheEntryUpdateArguments args)
    {
      
    }
  }
}
