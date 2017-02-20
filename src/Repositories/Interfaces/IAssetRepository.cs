using System.Threading.Tasks;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.TagFileAuth.Service.Repositories
{
  public interface IAssetRepository
  {
   // todo  Task<AssetDevice> GetAssetDevice(string radioSerial, string deviceType);

    Task<int> StoreAsset(IAssetEvent evt);
  }
 
}