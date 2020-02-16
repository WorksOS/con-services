using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace UnitTests.BSS_Tests
{
  public class BssAssetServiceExceptionFake : IBssAssetService
  {
    public bool WasExecuted { get; set; }

    public ExistingAssetDto GetAssetById(long assetId)
    {
      throw new NotImplementedException();
    }

    public ExistingAssetDto GetAssetBySerialNumberMakeCode(string serialNumber, string makeCode)
    {
      throw new NotImplementedException();
    }

    public Asset CreateAsset(AssetDto ibAsset, long deviceId, DeviceTypeEnum? installedDeviceType)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void AddAssetReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool UpdateAsset(long assetId, List<Param> modifiedProperties)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void UpdateAssetDeviceHistoryBssIds(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }

    public void UpdateAssetAliasBssIds(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }
  }
}