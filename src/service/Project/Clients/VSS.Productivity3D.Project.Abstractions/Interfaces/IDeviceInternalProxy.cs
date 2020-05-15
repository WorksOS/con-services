using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceInternalProxy : ICacheProxy
  {
    Task<DeviceData> GetDevice(string serialNumber, IDictionary<string, string> customHeaders = null);
    Task<ProjectDataResult> GetProjectsForDevice(string deviceUid, IDictionary<string, string> customHeaders = null);
  }
}
