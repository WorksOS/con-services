using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectAndAssetUidsExecutor : RequestExecutorContainer
  {

    ///  <summary>
    ///  There are 2 modes this may be called in:
    ///  a) Manual Import
    ///     a projectUid is provided, for whose account we determine if
    ///          an appropriate deviceLicenses is available:
    ///          note that the time of location is not limited to the project start/end time
    ///     if a serialNumber is provided and can be resolved,
    ///                the deviceUid will also be returned.
    /// 
    ///  b) Auto Import
    ///     a device is provided.
    ///     it's account is used to identify appropriate project.
    ///     A customers projects cannot overlap spatially at the same point-in-time
    ///                  therefore this should legitimately retrieve max of ONE match
    ///    
    ///     A standard (aka construction) project is only fair game if
    ///          a deviceUid is provided
    ///            and its account has deviceLicenses
    ///            and location is within it
    ///     Archived projects are not considered
    /// 
    ///  </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectAndAssetUidsRequest;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.FormatResult(uniqueCode: TagFileAuth.Models.ContractExecutionStatesEnum.SerializationError));
      }

      var projectAccountDeviceLicenseTotal = 0;
      ProjectData project = null;

      // manualImport, the project must be there and have deviceLicenses
      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        project = await dataRepository.GetProject(request.ProjectUid);
        log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded project? {JsonConvert.SerializeObject(project)}");

        if (project != null)
        {
          if (project.IsArchived)
          {
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 43);
          }
          projectAccountDeviceLicenseTotal = await dataRepository.GetDeviceLicenses(project.CustomerUID);
          log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded ProjectAccount deviceLicenses? {JsonConvert.SerializeObject(projectAccountDeviceLicenseTotal)}");
          if (projectAccountDeviceLicenseTotal < 1)
          {
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 38);
          }
        }
        else
        {
          return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 38);
        }
      }

      DeviceData device = null;
      // a CB will have a RadioSerial, whose suffix defines the type
      device = await dataRepository.GetDevice(request.RadioSerial);
      if (device == null)
       device = await dataRepository.GetDevice(request.Ec520Serial);

      if (!string.IsNullOrEmpty(request.ProjectUid))
      {
        return await HandleManualImport(request, project, device);
      }

      return await HandleAutoImport(request, device);
    }


    private async Task<GetProjectAndAssetUidsResult> HandleManualImport(GetProjectAndAssetUidsRequest request,
      ProjectData project, DeviceData device = null)
    {
      // by this stage...
      //  got a project,
      //  Can manually import tag files regardless if tag file time outside projectTime
      //  Can manually import tag files where we don't know the device

      var intersectingProjects = await dataRepository.CheckManualProjectIntersection(project, request.Latitude,
        request.Longitude, device);
      log.LogDebug(
        $"{nameof(HandleManualImport)}: Projects which intersect with manually imported project {JsonConvert.SerializeObject(intersectingProjects)}");

      if (!intersectingProjects.Any())
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 41);

      if (intersectingProjects.Count > 1)
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 49);

      return GetProjectAndAssetUidsResult.FormatResult(project.ProjectUID, device?.DeviceUID);
    }
    
    private async Task<GetProjectAndAssetUidsResult> HandleAutoImport(GetProjectAndAssetUidsRequest request,
      DeviceData device)
    {
      var potentialProjects = await dataRepository.CheckDeviceProjectIntersection(device, request.Latitude,
        request.Longitude, request.TimeOfPosition);
      log.LogDebug(
        $"{nameof(HandleAutoImport)}: GotPotentialProjects: {JsonConvert.SerializeObject(potentialProjects)}");

      if (!potentialProjects.Any())
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 48);
      }

      if (potentialProjects.Count > 1)
      {
        return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 49);
      }

      return GetProjectAndAssetUidsResult.FormatResult(potentialProjects[0].ProjectUID, device.DeviceUID);
    }
    
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
