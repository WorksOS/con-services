using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceProxy : ICacheProxy
  {
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<Guid> deviceUids, IHeaderDictionary customHeaders = null);
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<long> shortRaptorAssetIds, IHeaderDictionary customHeaders = null);
  }
}
