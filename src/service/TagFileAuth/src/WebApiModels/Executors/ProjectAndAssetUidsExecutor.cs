using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectAndAssetUidsExecutor : RequestExecutorContainer
  {
    public ICustomRadioSerialProjectMap CustomRadioSerialMapper { get; set; }

    ///  <summary>
    ///  There are 2 modes this may be called in:
    ///  a) manual a projectUid is provided 
    ///     check the project exists and the location is inside the project
    ///     check if device exists, but this is optional. Return deviceUID if we have it
    /// 
    ///  b) Auto/Direct Import only a deviceSerial provided.
    ///     check if device exists, obtaining the deviceUID 
    ///     get list of projects for the device and check lat/long inside one-only
    ///
    ///  TFA has the capability to be provided a radio/device type -> Asset/Project map to cover special cases
    ///  where a device has no provisioning but we want to bring it into a known project. In this case, if the
    ///  radio serial number and device type are found in the map, the item is processed as if were a manual
    ///  import into the project, under the asset, located in the map
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      }

      // Radio serial -> Asset/Project override
      if (CustomRadioSerialMapper.LocateAsset(request.RadioSerial, request.DeviceType, out var id))
      {
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: LocateAsset id {JsonConvert.SerializeObject(id)}");
        return GetProjectAndAssetUidsResult.FormatResult(id.ProjectUid.ToString(), id.AssetUid.ToString());
      }

      // Even for manual, we need to identify the device (if we can) so that TRex will assign events to the correct device.
      //   This comes into play when a user chooses a MachineName in a filter.
      // For manual, if we don't know the device, that's ok it will be stored within TRex as a JohnDoe.
      DeviceData device = null;
      if (!string.IsNullOrEmpty(request.RadioSerial))
      {
        device = await dataRepository.GetDevice(request.RadioSerial);
        var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by RadioSerial?: {request.RadioSerial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");
      }

      if ((device == null || device.Code != 0 || device.DeviceUID == null) && !string.IsNullOrEmpty(request.Ec520Serial))
      {
        device = await dataRepository.GetDevice(request.Ec520Serial);
        var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by Ec520Serial?: {request.Ec520Serial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");
      }

      if (!string.IsNullOrEmpty(request.ProjectUid))
        return await HandleManualImport(request, device);

      return HandleAutoImport(request, device);
    }


    private async Task<GetProjectAndAssetUidsResult> HandleManualImport(GetProjectAndAssetUidsRequest request, DeviceData device)
    {
      // no checking of device or project states or WM licensing at this stage

      var project = await dataRepository.GetProject(request.ProjectUid);
      log.LogDebug($"{nameof(HandleManualImport)}: Loaded project? {(project == null ? "project not found" : JsonConvert.SerializeObject(project))}");

      if (project == null)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 38);

      if (project.ProjectType != CwsProjectType.AcceptsTagFiles)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 48);

      // New requirement for WorksOS.
      if (project.IsArchived)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 43);

      // we need to retain ability to identify specific error codes 38, 43, 41, 48
      var intersects = PolygonUtils.PointInPolygon(project.ProjectGeofenceWKT, request.Latitude, request.Longitude);
      log.LogDebug($"{nameof(HandleManualImport)}: la/long is with project?: {intersects}");

      if (!intersects)
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device == null ? string.Empty : device.DeviceUID, uniqueCode: 41);

      return GetProjectAndAssetUidsResult.FormatResult(project.ProjectUID, device == null ? string.Empty : device.DeviceUID);
    }

    private GetProjectAndAssetUidsResult HandleAutoImport(GetProjectAndAssetUidsRequest request, DeviceData device)
    {
      // no checking of device or project states or WM licensing at this stage

      if (device == null || device.Code != 0 || device.DeviceUID == null)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: device?.Code ?? 47);

      var potentialProjects = dataRepository.GetIntersectingProjectsForDevice(device, request.Latitude, request.Longitude, out var errorCode);
      log.LogDebug($"{nameof(HandleAutoImport)}: GotIntersectingProjectsForDevice: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.ProjectDescriptors.Any())
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device.DeviceUID, uniqueCode: errorCode);

      if (potentialProjects.ProjectDescriptors.Count > 1)
        return GetProjectAndAssetUidsResult.FormatResult(assetUid: device.DeviceUID, uniqueCode: 49);

      return GetProjectAndAssetUidsResult.FormatResult(potentialProjects.ProjectDescriptors[0].ProjectUID, device.DeviceUID);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
