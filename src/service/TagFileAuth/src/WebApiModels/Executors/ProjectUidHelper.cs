using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  public class ProjectUidHelper
  {
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public static GetProjectAndAssetUidsResult FormatResult(string projectUid, string assetUid, int uniqueCode = 0)
    {
      return new GetProjectAndAssetUidsResult(projectUid, assetUid, 
        (uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode)),
        (uniqueCode == 0 ? ContractExecutionResult.DefaultMessage :
          (uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)))));
    }

    public static GetProjectAndAssetUidsCTCTResult FormatResult(string projectUid, string assetUid, string customerUid, bool hasValidSub, int uniqueCode = 0)
    {
      return new GetProjectAndAssetUidsCTCTResult(projectUid, assetUid, customerUid, hasValidSub,
        (uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode)),
        (uniqueCode == 0 ? ContractExecutionResult.DefaultMessage :
          (uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)))));
    }

    public static async Task<Tuple<string, string, List<Subscriptions>>> GetSNMAsset(ILogger log, DataRepository dataRepository, 
      string radioSerial, int deviceType, bool getAssetSubs)
    {
      var assetUid = string.Empty;
      var assetSubs = new List<Subscriptions>();
      string assetOwningCustomerUid = string.Empty;

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

      var result = new Tuple<string, string, List<Subscriptions>>(assetUid, assetOwningCustomerUid, assetSubs);
      return result;
    }

    public static async Task<Tuple<string, string, List<Subscriptions>>> GetEMAsset(ILogger log, DataRepository dataRepository,
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

      var result = new Tuple<string, string, List<Subscriptions>>(assetUid, assetOwningCustomerUid, assetSubs);
      return result;
    }

  }
}
