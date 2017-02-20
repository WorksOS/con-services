using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.TagFileAuth.Service.Models;
using VSS.TagFileAuth.Service.Repositories;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TagFileAuth.Service.MockClasses
{
  public class MockAssetRepository : IAssetRepository
  {
    private List<Asset> assets = new List<Asset>();

    public Task<int> StoreAsset(IAssetEvent evt)
    {
      if (evt is CreateAssetEvent)
      {
        var existingAsset = assets.Find(a => a.AssetUid == evt.AssetUID.ToString());
        if (existingAsset == null)
        {
          var ae = (CreateAssetEvent)evt;
          var newAsset = new Asset
          {
            AssetUid = ae.AssetUID.ToString(),
            LegacyAssetID = ae.LegacyAssetId,
            OwningCustomerUID = ae.OwningCustomerUID.HasValue ? ae.OwningCustomerUID.ToString() : "", 
            Name = ae.AssetName,
            IconKey = ae.IconKey,
            AssetType = string.IsNullOrEmpty(ae.AssetType) ? "Unassigned" : ae.AssetType,
            MakeCode = ae.MakeCode,
            Model = ae.Model,
            SerialNumber = ae.SerialNumber,
            LastActionedUtc = ae.ActionUTC
          };
          assets.Add(newAsset);
          return Task.FromResult(1);
        }
      }
      return Task.FromResult(0);
    }

    //public Task<AssetDevice> GetAssetDevice(string radioSerial, string deviceType)
    //{
    //  AssetDevice assetDevice = new AssetDevice();

    //  if (assets.Count > 0)
    //  {  
    //    assetDevice.AssetUid = assets[0].AssetUid;
    //    assetDevice.LegacyAssetId = assets[0].LegacyAssetID;
    //    assetDevice.OwningCustomerUid = assets[0].OwningCustomerUID;
    //    assetDevice.DeviceUid = Guid.NewGuid().ToString();
    //    assetDevice.DeviceType = DeviceTypeEnum.Series522.ToString();
    //    assetDevice.RadioSerial = "wetwet44"; // todo
    //  };
    //  return Task.FromResult(assetDevice);
      
    //}
  }
}
