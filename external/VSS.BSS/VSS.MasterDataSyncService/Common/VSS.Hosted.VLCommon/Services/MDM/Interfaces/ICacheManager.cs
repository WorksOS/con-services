using System.Runtime.Caching;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
  public interface ICacheManager
  {
    string GetCacheKey();
    string GetTokenFromCache(string cacheKey);
    void UpdateCacheItem(CacheItem cacheItem,int expiryTime);
  }
}
