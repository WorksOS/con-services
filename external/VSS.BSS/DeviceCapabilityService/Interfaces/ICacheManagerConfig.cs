using System.Collections.Specialized;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface ICacheManagerConfig
  {
    string CacheManagerName { get; }
    NameValueCollection Config { get; }
  }
}
