using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssAssetService : IBssAssetService
  {
    public ExistingAssetDto GetAssetById(long assetId)
    {
      var assetInfo = (from asset in Data.Context.OP.AssetReadOnly
                       let device = (from d in Data.Context.OP.DeviceReadOnly where d.ID == asset.fk_DeviceID select d).FirstOrDefault()
                       let owner = (from o in Data.Context.OP.CustomerReadOnly where o.BSSID == device.OwnerBSSID && device.ID != 0 select o).FirstOrDefault()
                       where asset.AssetID == assetId
                       select new
                                {
                                  Asset = asset,
                                  Device = device,
                                  Owner = owner
                                }).SingleOrDefault();

      var existingAsset = new ExistingAssetDto();

      if (assetInfo == null || assetInfo.Asset == null)
        return existingAsset;

      existingAsset.AssetId = assetInfo.Asset.AssetID;
      existingAsset.MapAsset(assetInfo.Asset);
      
      if (assetInfo.Device == null || assetInfo.Device.ID == 0)
        return existingAsset;

      existingAsset.DeviceId = assetInfo.Device.ID;
      existingAsset.Device.MapDevice(assetInfo.Device);

      if (assetInfo.Owner == null)
        return existingAsset;

      existingAsset.DeviceOwnerId = assetInfo.Owner.ID;
      existingAsset.DeviceOwner.MapOwner(assetInfo.Owner);

      if (existingAsset.DeviceOwner.Type == CustomerTypeEnum.Dealer)
      {
        existingAsset.DeviceOwner.RegisteredDealerId = assetInfo.Owner.ID;
        existingAsset.DeviceOwner.RegisteredDealerNetwork = (DealerNetworkEnum)assetInfo.Owner.fk_DealerNetworkID;
        return existingAsset;
      }

      var registeredDealer = Services.Customers().GetParentDealerByChildCustomerId(assetInfo.Owner.ID);

      if (registeredDealer != null && registeredDealer.Item1 != null && registeredDealer.Item1.IsActivated)
      {
        existingAsset.DeviceOwner.RegisteredDealerId = registeredDealer.Item1.ID;
        existingAsset.DeviceOwner.RegisteredDealerNetwork = (DealerNetworkEnum)registeredDealer.Item1.fk_DealerNetworkID;
      }

      return existingAsset;
    }

    public ExistingAssetDto GetAssetBySerialNumberMakeCode(string serialNumber, string makeCode)
    {
      var assetInfo = (from asset in Data.Context.OP.AssetReadOnly
                       let device = (from d in Data.Context.OP.DeviceReadOnly where d.ID == asset.fk_DeviceID select d).FirstOrDefault()
                       let owner = (from o in Data.Context.OP.CustomerReadOnly where o.BSSID == device.OwnerBSSID && device.ID != 0 select o).FirstOrDefault()
                       where asset.SerialNumberVIN == serialNumber 
                       && asset.fk_MakeCode == makeCode
                       select new
                       {
                         Asset = asset,
                         Device = device,
                         Owner = owner
                       }).SingleOrDefault();

      var existingAsset = new ExistingAssetDto();

      if (assetInfo == null || assetInfo.Asset == null)
        return existingAsset;

      existingAsset.AssetId = assetInfo.Asset.AssetID;
      existingAsset.MapAsset(assetInfo.Asset);

      if (assetInfo.Device == null || assetInfo.Device.ID == 0)
        return existingAsset;

      existingAsset.DeviceId = assetInfo.Device.ID;
      existingAsset.Device.MapDevice(assetInfo.Device);

      if (assetInfo.Owner == null)
        return existingAsset;

      existingAsset.DeviceOwnerId = assetInfo.Owner.ID;
      existingAsset.DeviceOwner.MapOwner(assetInfo.Owner);

      if (existingAsset.DeviceOwner.Type == CustomerTypeEnum.Dealer)
      {
        existingAsset.DeviceOwner.RegisteredDealerId = assetInfo.Owner.ID;
        existingAsset.DeviceOwner.RegisteredDealerNetwork = (DealerNetworkEnum)assetInfo.Owner.fk_DealerNetworkID;
        return existingAsset;
      }

      var registeredDealer = Services.Customers().GetParentDealerByChildCustomerId(assetInfo.Owner.ID);

      if (registeredDealer != null && registeredDealer.Item1 != null && registeredDealer.Item1.IsActivated)
      {
        existingAsset.DeviceOwner.RegisteredDealerId = registeredDealer.Item1.ID;
        existingAsset.DeviceOwner.RegisteredDealerNetwork = (DealerNetworkEnum)registeredDealer.Item1.fk_DealerNetworkID;
      }

      return existingAsset;
    }

    public Asset CreateAsset(AssetDto ibAsset, long deviceId, DeviceTypeEnum? deviceType)
    {

      Require.IsNotNull(ibAsset, "AssetDto");

      return API.Equipment.Create(
        Data.Context.OP,
        ibAsset.Name,
        ibAsset.MakeCode,
        ibAsset.SerialNumber,
        deviceId,
        deviceType.Value,
        string.Empty, //ibAsset.ProductFamily,
        ibAsset.Model,
        ibAsset.ManufactureYear ?? 0, AssetSequentialGuid.CreateGuid(), ibAsset.AssetVinSN, storeId:(int)StoreEnum.Trimble);
    }

    public bool UpdateAsset(long assetId, List<Param> modifiedProperties)
    {
      return API.Equipment.Update(Data.Context.OP, assetId, modifiedProperties);
    }

    public void AddAssetReference(IBssReference addBssReference, long storeId, string alias, string value, Guid uid)
    {
      addBssReference.AddAssetReference(storeId, alias, value, uid);
    }

    public void UpdateAssetDeviceHistoryBssIds(string oldBssId, string newBssId)
    {
      var assetDeviceHistories = (from a in Data.Context.OP.AssetDeviceHistory
                                  where a.OwnerBSSID == oldBssId
                                  select a).ToList();

      foreach (var assetDeviceHistory in assetDeviceHistories)
      {
        assetDeviceHistory.OwnerBSSID = newBssId;
      }

      var affectedRows = Data.Context.OP.SaveChanges();

      if (affectedRows != assetDeviceHistories.Count)
        throw new InvalidOperationException("Failed to save AssetDeviceHistory updates");
    }

    public void UpdateAssetAliasBssIds(string oldBssId, string newBssId)
    {
      var assetAliases = (from a in Data.Context.OP.AssetAlias
                          where a.OwnerBSSID == oldBssId
                          select a).ToList();

      foreach (var assetAlias in assetAliases)
      {
        assetAlias.OwnerBSSID = newBssId;
      }

      var affectedRows = Data.Context.OP.SaveChanges();

      if (affectedRows != assetAliases.Count)
        throw new InvalidOperationException("Failed to save AssetAlias updates");
    }

    private static readonly IUUIDSequentialGuid AssetSequentialGuid = new UUIDSequentialGuid();
  }
}
