
namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ICacheProxy
  {
    void ClearCacheItem<T>(string uid);
  }
}
