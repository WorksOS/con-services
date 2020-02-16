using System.Runtime.Caching;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface ICacheManager
  {
    string GetCacheKey();
    string GetTokenFromCache(string cacheKey);
    void UpdateCacheItem(CacheItem cacheItem,int expiryTime);
  }
}
