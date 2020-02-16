using System.Collections.Specialized;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Data
{
  public class CacheManagerConfig : ICacheManagerConfig
  {
    public CacheManagerConfig(string cacheManagerName, NameValueCollection config = null)
    {
      CacheManagerName = cacheManagerName;
      Config = config;
    }

    public string CacheManagerName { get; private set; }
    public NameValueCollection Config { get; private set; }
  }
}
