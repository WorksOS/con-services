using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectIdExecutor : RequestExecutorContainer
  {
    ///  <summary>
    ///  Processes the get project id request and finds the id of the project corresponding to the given location and asset/tccorgID and relavant subscriptions.
    ///  
    ///   A device asset reports at a certain location, at a point in time -
    ///         which project should its data be accumulating into?
    ///     assumption1: A customers projects cannot overlap spatially at the same point-in-time
    ///                  this applies to construction and Landfill types
    ///                  therefore this should legitimately retrieve max of ONE match
    ///     assumption2: tag files are data type-generic at this level, so this function does not need to
    ///                  differentiate between the 3 subscription types.
    ///     assumption3: the customer must be identifiable by EITHER the AssetID, or TCCOrgID being supplied
    ///                  only projects for that customer are fair game.
    ///    
    ///     A construction project is only fair game if an assetID is provided
    ///     A landfill project is fair game for an aasetID or a TCCOrgID
    /// 
    ///     determine the union (ONE) of the following:
    ///     1) which projects were valid at this time?
    ///     2) which customers have a machineControl-type subscription for at this time? (for construction type projects)
    ///         a) for the asset provided OR
    ///         b) any assets if -1 is provided
    ///     3) which project.sites are these points are in?
    ///     
    ///  </summary>
    ///  <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectIdResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectIdRequest;
      long projectId = -1;
      var potentialProjects = new List<Project.Abstractions.Models.DatabaseModels.Project>();

      IEnumerable<Subscriptions> assetSubs = null;
      CustomerTccOrg customerTCCOrg = null;
      CustomerTccOrg customerAssetOwner = null;

      // must be able to find one or other customer for a) tccOrgUid b) legacyAssetID, whichever is provided
      if (!string.IsNullOrEmpty(request.tccOrgUid))
      {
        customerTCCOrg = await dataRepository.LoadCustomerByTccOrgId(request.tccOrgUid);
        log.LogDebug(
          $"{nameof(ProjectIdExecutor)}: Loaded CustomerByTccOrgId? {JsonConvert.SerializeObject(customerTCCOrg)}");
      }

      // assetId could be valid (>0) or -1 (john doe i.e. landfill) or -2 (imported tagfile)
      if (request.assetId > 0)
      {
        var asset = await dataRepository.LoadAsset(request.assetId);
        log.LogDebug($"{nameof(ProjectIdExecutor)}: Loaded asset? {JsonConvert.SerializeObject(asset)}");

        if (asset != null && !string.IsNullOrEmpty(asset.OwningCustomerUID))
        {
          customerAssetOwner = await dataRepository.LoadCustomerByCustomerUIDAsync(asset.OwningCustomerUID);
          log.LogDebug(
            $"{nameof(ProjectIdExecutor)}: Loaded customerAssetOwner? {JsonConvert.SerializeObject(customerAssetOwner)}");

          assetSubs = await dataRepository.LoadAssetSubs(asset.AssetUID, request.timeOfPosition);
          log.LogDebug($"{nameof(ProjectIdExecutor)}: Loaded assetSubs? {JsonConvert.SerializeObject(assetSubs)}");
        }
      }

      if (customerTCCOrg != null || customerAssetOwner != null)
      {
        //Look for projects with (request.latitude, request.longitude) inside their boundary
        //and belonging to customers who have a Project Monitoring subscription
        //for asset with id request.assetId at time request.timeOfPosition 
        //and the customer owns the asset. (In VL multiple customers can have subscriptions
        //for an asset but only the owner gets the tag file data).

        //  standard 2d / 3d project aka construction project
        //    IGNORE any tccOrgID
        //    must have valid assetID, which must have a 3d sub.
        if (customerAssetOwner != null && assetSubs != null && assetSubs.Any())
        {
          var standardProjects = await dataRepository.GetStandardProject(customerAssetOwner.CustomerUID,
            request.latitude, request.longitude, request.timeOfPosition);
          if (standardProjects.Any())
          {
            potentialProjects.AddRange(standardProjects);
            log.LogDebug(
              $"{nameof(ProjectIdExecutor)}: Loaded standardProjects which lat/long is within {JsonConvert.SerializeObject(standardProjects)}");
          }
          else
          {
            log.LogDebug($"{nameof(ProjectIdExecutor)}: No standardProjects loaded");
          }
        }

        // ProjectMonitoring project
        //  MUST have a TCCOrgID
        //  customersOrgID must have a PM sub
        //  allow johnDoe assets(-1) and valid assetIDs(no manually imported tagfile(assetid = -2))
        if (customerTCCOrg != null && request.assetId != -2)
        {
          var pmProjects = await dataRepository.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
            request.latitude, request.longitude, request.timeOfPosition,
            (int) ProjectType.ProjectMonitoring, (int) ServiceTypeEnum.ProjectMonitoring);
          if (pmProjects.Any())
          {
            potentialProjects.AddRange(pmProjects);
            log.LogDebug(
              $"{nameof(ProjectIdExecutor)}: Loaded pmProjects which lat/long is within {JsonConvert.SerializeObject(pmProjects)}");
          }
          else
          {
            log.LogDebug($"{nameof(ProjectIdExecutor)}: No pmProjects loaded");
          }
        }

        // Landfill project
        //   MUST have a TCCOrgID
        //   customersOrgID must have a PM sub
        //   allow manual assets(-2) and johnDoe assets(-1) and valid assetIDs
        if (customerTCCOrg != null)
        {
          var landfillProjects = await dataRepository.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
            request.latitude, request.longitude, request.timeOfPosition,
            (int) ProjectType.LandFill, (int) ServiceTypeEnum.Landfill);
          if (landfillProjects.Any())
          {
            potentialProjects.AddRange(landfillProjects);
            log.LogDebug(
              $"{nameof(ProjectIdExecutor)}: Loaded landfillProjects which lat/long is within {JsonConvert.SerializeObject(landfillProjects)}");
          }
          else
          {
            log.LogDebug($"{nameof(ProjectIdExecutor)}: No landfillProjects loaded");
          }
        }

        //projectId
        //If zero found then If manualAsset returns -3 else returns -1
        //If one found then returns its id
        //If > 1 found then returns -2
        if (!potentialProjects.Any())
          switch (request.assetId)
          {
            case -2:
              projectId = -3;
              break;
            default:
              projectId = -1;
              break;
          }
        else if (potentialProjects.Distinct().Count() > 1)
          projectId = -2;
        else
          projectId = potentialProjects[0].LegacyProjectID;
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
