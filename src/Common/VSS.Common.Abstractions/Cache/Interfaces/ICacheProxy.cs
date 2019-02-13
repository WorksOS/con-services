namespace VSS.Common.Abstractions.Cache.Interfaces
{
  public interface ICacheProxy
  {
    void ClearCacheItem(string uid, string userId=null);
  }
}