using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces.Repository
{
  public interface IDeviceRepository
  {
    Task<Device> GetDevice(string deviceUid);
    Task<Device> GetDevice(int shortRaptorAssetId);

    Task<int> StoreEvent(IDeviceEvent evt);
  }
}
