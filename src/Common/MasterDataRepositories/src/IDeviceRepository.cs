using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IDeviceRepository
  {
    Task<Device> GetDevice(string deviceUid);
    Task<Device> GetDevice(int shortRaptorAssetId);

    Task<int> StoreEvent(IDeviceEvent evt);
  }
}
