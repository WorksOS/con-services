using System.Collections.Specialized;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface ICacheManagerConfig
  {
    string CacheManagerName { get; }
    NameValueCollection Config { get; }
  }
}
