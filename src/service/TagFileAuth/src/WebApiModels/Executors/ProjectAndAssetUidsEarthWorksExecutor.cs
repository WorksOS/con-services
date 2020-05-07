using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which tries to identify a project for the location,
  ///      for use by CTCT EarthWorks devices to obtain cutfill map from 3dp.
  /// The customer, for which projects are fair game, can be determined from
  ///     1) SNM serialNumber
  ///     2) EC520 serialNumber
  ///     3) tccOrgId  
  /// The commercial model re servicePlans has not been established,
  ///      it MAY be that if an asset found but has no service plan,
  ///                      then only surveyedSurface ground is provided (no productionData)
  ///                      else production data AND SS is provided
  ///      don't know what it would be for landfills and civil project using a TCCOrgId
  /// </summary>
  public class ProjectAndAssetUidsEarthWorksExecutor : RequestExecutorContainer
  {
    ///  <summary>
    ///  Processes the get project Uid request and finds the Uid of the project corresponding to the given location and devices Customer and relevant deviceLicenses.
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsEarthWorksRequest;
      if (request == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));

      // a CB will have a RadioSerial, whose suffix defines the type.
      //    however we probably don't need this as cws has a lookup by serialNumber only,
      //    and due to suffixes, these should be unique over CB/EC
      var device = await dataRepository.GetDevice(request.RadioSerial);
      var deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: { contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
      log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by RadioSerial?: {request.RadioSerial} device: { JsonConvert.SerializeObject(device)} {deviceStatus}");

      if (device == null || device?.Code != 0 || device?.DeviceUID == null)
      {
        device = await dataRepository.GetDevice(request.Ec520Serial);
        deviceStatus = (device?.Code == 0) ? string.Empty : $"Not found: deviceErrorCode: {device?.Code} message: {contractExecutionStatesEnum.FirstNameWithOffset(device?.Code ?? 0)}";
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Found by Ec520Serial?: {request.Ec520Serial} device: {JsonConvert.SerializeObject(device)} {deviceStatus}");
      }

      if (device == null || device?.Code != 0 || device?.DeviceUID == null)
        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: string.Empty, customerUid: string.Empty, uniqueCode: device?.Code ?? 33);

      return await HandleCutFillExport(request, device);
    }

    /// <summary>
    /// EarthWorks cut/fill doesn't REQUIRE a subscription.
    /// </summary>
    private async Task<GetProjectAndAssetUidsEarthWorksResult> HandleCutFillExport(GetProjectAndAssetUidsEarthWorksRequest request,
      DeviceData device)
    {
      var errorCode = 0;
      var potentialProjects = dataRepository.GetIntersectingProjectsForDevice(device, request.Latitude, request.Longitude, out errorCode);
      log.LogDebug(
        $"{nameof(HandleCutFillExport)}: GotPotentialProjects: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.ProjectDescriptors.Any())
        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: device.DeviceUID, customerUid: device.CustomerUID, uniqueCode: errorCode);

      if (potentialProjects.ProjectDescriptors.Count > 1)
        return GetProjectAndAssetUidsEarthWorksResult.FormatResult(assetUid: device.DeviceUID, customerUid: potentialProjects.ProjectDescriptors[0].CustomerUID, hasValidSub: true, uniqueCode: 49);
      
      var deviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.CustomerUID);
      return GetProjectAndAssetUidsEarthWorksResult.FormatResult(
        potentialProjects.ProjectDescriptors[0].ProjectUID, device.DeviceUID,
        potentialProjects.ProjectDescriptors[0].CustomerUID,
        (deviceLicenseTotal > 0));
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
