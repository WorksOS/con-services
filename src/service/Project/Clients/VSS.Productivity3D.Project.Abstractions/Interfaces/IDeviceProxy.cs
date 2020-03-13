using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IDeviceProxy : ICacheProxy
  {
    Task<DeviceData> GetDevice(string serialNumber);
  }
}
