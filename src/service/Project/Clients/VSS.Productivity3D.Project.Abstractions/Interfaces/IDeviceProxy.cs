using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceProxy : ICacheProxy
  {  
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<Guid> deviceUids, IDictionary<string, string> customHeaders = null);
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<long> shortRaptorAssetIds, IDictionary<string, string> customHeaders = null);
  }
}
