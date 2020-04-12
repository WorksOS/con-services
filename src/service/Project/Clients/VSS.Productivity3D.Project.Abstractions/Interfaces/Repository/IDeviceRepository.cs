using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces.Repository
{
  public interface IDeviceRepository
  {
    Task<Device> GetDevice(string deviceUid);
    Task<Device> GetDevice(int shortRaptorAssetId);

    Task<List<Device>> GetDevices(IEnumerable<Guid> deviceUids);
    Task<List<Device>> GetDevices(IEnumerable<long> shortRaptorAssetIds);

    Task<int> StoreEvent(IDeviceEvent evt);
  }
}
