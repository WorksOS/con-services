using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  public static class ProjectUidHelper
  {
    public static async Task<(string assetUid, string assetOwningCustomerUid, List<Subscriptions> assetSubs)> GetSNMAsset(ILogger log, DataRepository dataRepository, 
      string radioSerial, int deviceType, bool getAssetSubs)
    {
      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      var assetOwningCustomerUid = string.Empty;

      var assetDevice = await dataRepository.LoadAssetDevice(radioSerial, ((DeviceTypeEnum) deviceType).ToString());

      // special case in CGen US36833 If fails on DT SNM940 try as again SNM941 
      if (assetDevice == null && (DeviceTypeEnum) deviceType == DeviceTypeEnum.SNM940)
      {
        log.LogDebug($"{nameof(GetSNMAsset)}: Failed for SNM940 trying again as Device Type SNM941");
        assetDevice = await dataRepository.LoadAssetDevice(radioSerial, DeviceTypeEnum.SNM941.ToString());
      }

      if (assetDevice != null)
      {
        assetUid = assetDevice.AssetUID;
        assetOwningCustomerUid = assetDevice.OwningCustomerUID;
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded assetDevice {JsonConvert.SerializeObject(assetDevice)}");

        if (getAssetSubs)
        {
          assetSubs = (await dataRepository.LoadAssetSubs(assetUid, DateTime.UtcNow));
          log.LogDebug($"{nameof(GetSNMAsset)}: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");
        }
      }
      else
      {
        log.LogDebug($"{nameof(GetSNMAsset)}: Unable to locate SNM assetDevice for radioSerial: {radioSerial} and deviceType: {deviceType}");
      }

      return (assetUid: assetUid, assetOwningCustomerUid: assetOwningCustomerUid, assetSubs: assetSubs);
    }

    public static async Task<(string assetUid, string assetOwningCustomerUid, List<Subscriptions> assetSubs)> GetEMAsset(ILogger log, DataRepository dataRepository,
      string serialNumber, int deviceType, bool getAssetSubs)
    {
      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      string assetOwningCustomerUid = string.Empty;

      var assetDevice = await dataRepository.LoadAssetDevice(serialNumber, ((DeviceTypeEnum)deviceType).ToString());
      if (assetDevice != null)
      {
        assetUid = assetDevice.AssetUID;
        assetOwningCustomerUid = assetDevice.OwningCustomerUID;
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded assetDevice {JsonConvert.SerializeObject(assetDevice)}");

        if (getAssetSubs)
        {
          assetSubs = (await dataRepository.LoadAssetSubs(assetUid, DateTime.UtcNow));
          log.LogDebug($"{nameof(GetSNMAsset)}: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");
        }
      }
      else
      {
        log.LogDebug($"{nameof(GetSNMAsset)}: Unable to locate SNM assetDevice for radioSerial: {serialNumber} and deviceType: {deviceType}");
      }

      return (assetUid: assetUid, assetOwningCustomerUid: assetOwningCustomerUid, assetSubs: assetSubs);
    }

  }
}
