
using System.Collections.Generic;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ICacheProxy
  {
    void ClearCacheItem(string uid, IDictionary<string, string> customHeaders);
  }
}
