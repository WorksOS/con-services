using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceProxy : ICacheProxy
  {
    Task<DeviceDataSingleResult> GetDevice(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<DeviceDataSingleResult> GetDevice(int shortRaptorAssetId, IDictionary<string, string> customHeaders = null);
    Task<ProjectDataResult> GetProjects(string deviceUid, IDictionary<string, string> customHeaders = null);
  }
}
