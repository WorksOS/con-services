using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssAssetService  
  {
    ExistingAssetDto GetAssetById(long assetId);
    ExistingAssetDto GetAssetBySerialNumberMakeCode(string serialNumber, string makeCode);
    Asset CreateAsset(AssetDto assetDto, long installedDeviceId, DeviceTypeEnum? installedDeviceType);
    void AddAssetReference(IBssReference addBssReference, long storeId, string alias , string value, Guid uid);
    bool UpdateAsset(long assetId, List<Param> modifiedProperties);
    void UpdateAssetDeviceHistoryBssIds(string oldBssId, string newBssId);
    void UpdateAssetAliasBssIds(string oldBssId, string newBssId);
  }
}