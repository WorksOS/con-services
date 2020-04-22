using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceProxy : ICacheProxy
  {
    Task<DeviceDataResult> GetDevice(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<DeviceData> GetDevice(int shortRaptorAssetId, IDictionary<string, string> customHeaders = null);
    Task<List<ProjectData>> GetProjectsForDevice(string deviceUid, IDictionary<string, string> customHeaders = null);

    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<Guid> deviceUids, IDictionary<string, string> customHeaders = null);
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingDevices(List<long> shortRaptorAssetIds, IDictionary<string, string> customHeaders = null);
  }
}
