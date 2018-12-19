using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IDeviceRepository
  {
    Task<Device> GetDevice(string deviceUid);
    Task<AssetDeviceIds> GetAssociatedAsset(string radioSerial, string deviceType);

    Task<int> StoreEvent(IDeviceEvent evt);
  }
}
