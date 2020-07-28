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

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectUidsExecutor : RequestExecutorContainer
  {
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
    ///  TFA v5 no longer has radio/deviceType -> Asset/Project map to cover special cases
    ///     Radio serial no longer supported as not available in cws
    ///
    /// todoJeannie remove customSerialNumberMapping
    /// todoJeannie remove old execs etc
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectUidsResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      }

      var device = await dataRepository.GetDevice(request.PlatformSerial);
        var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by PlatformSerial?: {request.PlatformSerial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");

      if (!string.IsNullOrEmpty(request.ProjectUid))
        return await HandleManualImport(request, device);

      return HandleAutoImport(request, device);
    }

    private async Task<GetProjectUidsResult> HandleManualImport(GetProjectUidsRequest request, DeviceData device)
    {
      // no checking of device or project states or WM licensing at this stage
      var deviceUid = device == null ? string.Empty : device.DeviceUID;
      var customerUid = device == null ? string.Empty : device.CustomerUID;
      var project = await dataRepository.GetProject(request.ProjectUid);
      log.LogDebug($"{nameof(HandleManualImport)}: Loaded project? {(project == null ? "project not found" : JsonConvert.SerializeObject(project))}");

      if (project == null)
        return GetProjectUidsResult.FormatResult(deviceUid: deviceUid, customerUid: customerUid, uniqueCode: 38);

      if (!project.ProjectType.HasFlag(CwsProjectType.AcceptsTagFiles))
        return GetProjectUidsResult.FormatResult(deviceUid: deviceUid, customerUid: customerUid, uniqueCode: 53);

      // New requirement for WorksOS.
      if (project.IsArchived)
        return GetProjectUidsResult.FormatResult(deviceUid: deviceUid, customerUid: customerUid, uniqueCode: 43);

      // we need to retain ability to identify specific error codes 38, 43, 41, 53
      if (!request.HasLatLong && request.HasNE && request.Northing != null && request.Easting != null)
      {
        var convertedLL = await dataRepository.ConvertNEtoLL(request.ProjectUid, request.Northing.Value, request.Easting.Value);
        if (convertedLL == null)
          return GetProjectUidsResult.FormatResult(deviceUid: deviceUid, customerUid: customerUid, uniqueCode: 18);
        request.Longitude = convertedLL.ConversionCoordinates[0].X;
        request.Latitude = convertedLL.ConversionCoordinates[0].Y;
      }

      var intersects = PolygonUtils.PointInPolygon(project.ProjectGeofenceWKT, request.Latitude, request.Longitude);
      log.LogDebug($"{nameof(HandleManualImport)}: la/long is with project?: {intersects}");

      if (!intersects)
        return GetProjectUidsResult.FormatResult(deviceUid: deviceUid, customerUid: customerUid, uniqueCode: 41);

      return GetProjectUidsResult.FormatResult(project.ProjectUID, deviceUid, customerUid);
    }

    private GetProjectUidsResult HandleAutoImport(GetProjectUidsRequest request, DeviceData device)
    {
      if (device == null || device.Code != 0 || device.DeviceUID == null)
        return GetProjectUidsResult.FormatResult(uniqueCode: device?.Code ?? 47);

      var potentialProjects = dataRepository.GetIntersectingProjectsForDevice(request, device, out var errorCode);

      log.LogDebug($"{nameof(HandleAutoImport)}: GotIntersectingProjectsForDevice: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.ProjectDescriptors.Any())
        return GetProjectUidsResult.FormatResult(deviceUid: device.DeviceUID, customerUid: device.CustomerUID, uniqueCode: errorCode);

      if (potentialProjects.ProjectDescriptors.Count > 1)
        return GetProjectUidsResult.FormatResult(deviceUid: device.DeviceUID, customerUid: device.CustomerUID, uniqueCode: 49);

      return GetProjectUidsResult.FormatResult(potentialProjects.ProjectDescriptors[0].ProjectUID, device.DeviceUID, device.CustomerUID);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
