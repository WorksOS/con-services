using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceInternalProxy : ICacheProxy
  {
    Task<DeviceData> GetDevice(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<DeviceData> GetDevice(int shortRaptorAssetId, IDictionary<string, string> customHeaders = null);
    Task<List<ProjectData>> GetProjectsForDevice(string deviceUid, IDictionary<string, string> customHeaders = null);
  }
}
