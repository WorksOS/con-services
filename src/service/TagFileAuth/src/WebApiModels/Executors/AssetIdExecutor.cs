using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using TagFileDeviceTypeEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums.TagFileDeviceTypeEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets a shortRaptorAssetId and/or serviceType
  ///     for the requested serialNumber and/or shortRaptorProjectId.
  /// </summary>
  public class AssetIdExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetAssetIdRequest;
      if (request == null) return null;

      long shortRaptorAssetId = -1;
      var serviceType = 0;

      // for WorksOS, anyone can do a manualImport
      // I believe that when shortRaptorProjectId is provided , TagFileDeviceTypeEnum will always be ManualImport
      //     so don't need to lookup shortRaptorAssetId
      if (request.shortRaptorProjectId > 0)
      {
        var project = await dataRepository.GetProject(request.shortRaptorProjectId);
        log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          // CCSSSCON-207 I believe deviceType will always be Manual when shortRaptorProjectId is provided 
          //  If a projects account (for manual import) has only a free sub, I don't believe the UI should allow it to get here.
          //  However, if it does, then should we allow manually importing tag files into project where the account has no deviceLicenses? 
          //   Assuming no here.
          var projectAccountLicenseTotal = await dataRepository.GetDeviceLicenses(project.CustomerUID);
          if (request.deviceType == (int) TagFileDeviceTypeEnum.ManualImport || projectAccountLicenseTotal > 0)
            serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").CGEnum; 
          else
            log.LogDebug($"{nameof(AssetIdExecutor)}: Project found, however it's customer has no device licenses: {projectAccountLicenseTotal} or not a manual deviceType: {request.deviceType.ToString()}");
        }

        log.LogDebug($"{nameof(AssetIdExecutor)}: ManualImport return: shortRaptorAssetId {shortRaptorAssetId} serviceType {serviceType}");
        return GetAssetIdResult.CreateGetAssetIdResult(serviceType == 0, shortRaptorAssetId, serviceType);
      }
      

      if (string.IsNullOrEmpty(request.serialNumber))
      {
        log.LogDebug($"{nameof(AssetIdExecutor)}: Not a manualImport and no serialNumber, therefore nothing to identify with.");
        return GetAssetIdResult.CreateGetAssetIdResult(false, shortRaptorAssetId, serviceType);
      }

      // Auto or Direct import i.e. no shortRaptorProjectId
      // try to identify the device by it's serialNumber in cws. Need to get CustomerUid (cws) and shortRaptorAssetId (localDB)
      var device = await dataRepository.GetDevice(request.serialNumber);
      log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded device? {JsonConvert.SerializeObject(device)}");

      if (device?.Code == 0)
      {
        if (String.Compare(device.RelationStatus.ToString().ToUpper(), "ACTIVE", StringComparison.OrdinalIgnoreCase) == 0)
        {
          shortRaptorAssetId = device.ShortRaptorAssetId ?? -1;
          // CCSSSCON-207 If a devices account has only a free sub, then should we import tag files into it?
          int deviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.CustomerUID);
          if (deviceLicenseTotal > 0)
            serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").CGEnum;
          else
            log.LogDebug($"{nameof(AssetIdExecutor)}: Device found, however it's customer has no device licenses: {deviceLicenseTotal}");
        }
        else
          log.LogDebug($"{nameof(AssetIdExecutor)}: Device found, however status is not active");
      }

      log.LogDebug($"{nameof(AssetIdExecutor)}: Auto/Direct Import return: shortRaptorAssetId {shortRaptorAssetId} serviceType {serviceType}");
      var result = !((shortRaptorAssetId == -1) && (serviceType == 0));
      return GetAssetIdResult.CreateGetAssetIdResult(result, shortRaptorAssetId, serviceType);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
