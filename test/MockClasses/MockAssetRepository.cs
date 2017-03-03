using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Repositories;
using Repositories.DBModels;

namespace VSS.TagFileAuth.Service.MockClasses
{
  public class MockAssetRepository : AssetRepository
  {
    private List<Asset> assets = new List<Asset>();

    public MockAssetRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString, logger)
    {
    }

    public Task<int> StoreAsset(IAssetEvent evt)
    {
      if (evt is CreateAssetEvent)
      {
        var existingAsset = assets.Find(a => a.AssetUID == evt.AssetUID.ToString());
        if (existingAsset == null)
        {
          var ae = (CreateAssetEvent)evt;
          var newAsset = new Asset
          {
            AssetUID = ae.AssetUID.ToString(),
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
