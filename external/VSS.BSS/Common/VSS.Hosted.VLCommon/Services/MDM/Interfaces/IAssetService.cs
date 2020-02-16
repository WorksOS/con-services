
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
  public interface IAssetService
  {
    bool CreateAsset(object assetDetails);
    bool UpdateAsset(object assetDetails);
    
  }
}