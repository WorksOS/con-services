using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
    public interface IDeviceService
    {
        bool CreateDevice(object deviceDetails);
        bool UpdateDevice(object deviceDetails);
        bool AssociateDeviceAsset(AssociateDeviceAssetEvent assetDeviceDetails);
        bool DissociateDeviceAsset(DissociateDeviceAssetEvent assetDeviceDetails);
        bool ReplaceDevice(DeviceReplacementEvent assetDeviceDetails);
    }
}
