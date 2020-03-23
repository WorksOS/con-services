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
          // todoMaverick If a projects account (for manual import) has only a free sub, then should we import tag files into it?
          //int projectAccountEntitlement = await dataRepository.GetDeviceLicenses(project.CustomerUID);

          // todoMaverick I believe this is always true when shortRaptorProjectId is provided 
          if (request.deviceType == (int) TagFileDeviceTypeEnum.ManualImport)
          {
            serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").CGEnum; ;
          }
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
      // try to identify the device by it's serialNumber.
      var device = await dataRepository.GetDevice(request.serialNumber);
      log.LogDebug($"{nameof(AssetIdExecutor)}: Loaded device? {JsonConvert.SerializeObject(device)}");

      if (device != null)
      {
        shortRaptorAssetId = device.DeviceDescriptor.ShortRaptorAssetId ?? -1;
        // todoMaverick If a devices account has only a free sub, then should we import tag files into it?
        int deviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.DeviceDescriptor.CustomerUID);
        if (deviceLicenseTotal > 0)
          serviceType = serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").CGEnum;
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
