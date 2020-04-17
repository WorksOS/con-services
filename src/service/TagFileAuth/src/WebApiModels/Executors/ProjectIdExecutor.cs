using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectIdExecutor : RequestExecutorContainer
  {
    ///  <summary>
    ///  Processes the raptor get project id request and finds the shortRaptorProjectId of any projects
    ///      which belong to the devices owningCustomerUid,
    ///          for the location, time
    ///          and that the device is assigned to,
    ///          and that the device is licensed (and claimed? CCSSSCON-207).
    ///  
    ///  assumption: A customers projects cannot overlap spatially at the same point-in-time
    ///                  - this is controlled on project creation
    ///              therefore this should legitimately retrieve max of ONE match
    ///  </summary>
    ///  <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectIdResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectIdRequest;
      if (request == null) return null;
      long projectId = -1;

      // assetId could be valid (>0) or -1 (john doe i.e. landfill) or -2 (imported tagfile)
      if (request.shortRaptorAssetId > 0)
      {
        var device = await dataRepository.GetDevice((int) request.shortRaptorAssetId);
        log.LogDebug($"{nameof(ProjectIdExecutor)}: Loaded device? {JsonConvert.SerializeObject(device)}");

        var deviceLicenseTotal = 0;
        if (device != null)
          deviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.CustomerUID);

        if (device == null || deviceLicenseTotal < 1)
          return GetProjectIdResult.CreateGetProjectIdResult(false, projectId);

        var potentialProjects = await dataRepository.GetIntersectingProjectsForDevice(device,
          request.latitude, request.longitude, request.timeOfPosition);
        log.LogDebug($"{nameof(ProjectIdExecutor)}: Loaded projects which lat/long is within {JsonConvert.SerializeObject(potentialProjects)}");
         

        //projectId
        //If zero found then returns -1
        //If one found then returns its id
        //If > 1 found then returns -2
        if (!potentialProjects.Any())
          projectId = -1;
        else if (potentialProjects.Count > 1)
          projectId = -2;
        else
          projectId = potentialProjects[0].ShortRaptorProjectId;
      }

      var result = projectId > 1;
      return GetProjectIdResult.CreateGetProjectIdResult(result, projectId);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
