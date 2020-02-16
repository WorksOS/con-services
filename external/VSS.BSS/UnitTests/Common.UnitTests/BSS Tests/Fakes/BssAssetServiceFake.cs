using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace UnitTests.BSS_Tests
{
  public class BssAssetServiceFake : IBssAssetService
  {
    private readonly ExistingAssetDto _existingAssetToReturn;
    private string _model;
    private string _productFamily;
    private readonly Asset _assetToReturn;
    private readonly bool _booleanToReturn;

    public bool WasExecuted { get; set; }

    public long AssetIdArg { get; set; }
    public List<Param> ModifiedPropertiesArg { get; set; }

    public BssAssetServiceFake(ExistingAssetDto existingAssetToReturn)
    {
      _existingAssetToReturn = existingAssetToReturn;
    }

    public BssAssetServiceFake(string model, string productFamily)
    {
      _model = model;
      _productFamily = productFamily;
    }

    public BssAssetServiceFake(Asset assetToReturn)
    {
      _assetToReturn = assetToReturn;
    }

    public BssAssetServiceFake(bool booleanToReturn)
    {
      _booleanToReturn = booleanToReturn;
    }

    public ExistingAssetDto GetAssetById(long assetId)
    {
      WasExecuted = true;
      return _existingAssetToReturn;
    }

    public ExistingAssetDto GetAssetBySerialNumberMakeCode(string serialNumber, string makeCode)
    {
      WasExecuted = true;
      return _existingAssetToReturn;
    }

    public Asset CreateAsset(AssetDto ibAsset, long deviceId, DeviceTypeEnum? installedDeviceType)
    {
      WasExecuted = true;
      return _assetToReturn;
    }

    public void AddAssetReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      WasExecuted = true;
    }

    public bool UpdateAsset(long assetId, List<Param> modifiedProperties)
    {
      AssetIdArg = assetId;
      ModifiedPropertiesArg = modifiedProperties;
      WasExecuted = true;
      return _booleanToReturn;
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