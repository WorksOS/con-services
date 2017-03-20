using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApiModels.ResultHandling;
using WebApiModels.Models;
using WebApiModels.Enums;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets the project id of the project for the requested asset location and date time.
  /// </summary>
  public class ProjectIdExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project id request and finds the id of the project corresponding to the given location and asset/tccorgID and relavant subscriptions.
    /// 
    ///  A device asset reports at a certain location, at a point in time -
    ///        which project should its data be accumulating into?
    ///    assumption1: A customers projects cannot overlap spatially at the same point-in-time
    ///                 this applies to construction and Landfill types
    ///                 therefore this should legitimately retrieve max of ONE match
    ///    assumption2: tag files are data type-generic at this level, so this function does not need to
    ///                 differentiate between the 3 subscription types.
    ///    assumption3: the customer must be identifiable by EITHER the AssetID, or TCCOrgID being supplied
    ///                 only projects for that customer are fair game.
    ///   
    ///    A construction project is only fair game if an assetID is provided
    ///    A landfill project is fair game for an aasetID or a TCCOrgID
    ///
    ///    determine the union (ONE) of the following:
    ///    1) which projects were valid at this time?
    ///    2) which customers have a machineControl-type subscription for at this time? (for construction type projects)
    ///        a) for the asset provided OR
    ///        b) any assets if -1 is provided
    ///    3) which project.sites are these points are in?
    ///    
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectIdResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectIdRequest request = item as GetProjectIdRequest;
      log.LogDebug("ProjectIdExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      long projectId = -1; 
      IEnumerable<Project> potentialProjects = null;

      Asset asset = null;
      IEnumerable<Subscriptions> assetSubs = null;
      CustomerTccOrg customerTCCOrg = null;
      CustomerTccOrg customerAssetOwner = null;

      // must be able to find one or other customer for a) tccOrgUid b) legacyAssetID, whichever is provided
      if (!string.IsNullOrEmpty(request.tccOrgUid))
      {
        customerTCCOrg = dataRepository.LoadCustomerByTccOrgId(request.tccOrgUid);
        log.LogDebug("ProjectIdExecutor: Loaded CustomerByTccOrgId? {0}", JsonConvert.SerializeObject(customerTCCOrg));
      }

      // assetId could be valid (>0) or -1 (john doe i.e. landfill) or -2 (imported tagfile)
      if (request.assetId > 0)
      {
        asset = dataRepository.LoadAsset(request.assetId);
        log.LogDebug("ProjectIdExecutor: Loaded asset? {0}", JsonConvert.SerializeObject(asset));

        if (asset != null && !string.IsNullOrEmpty(asset.OwningCustomerUID))
        {
          customerAssetOwner = dataRepository.LoadCustomerByCustomerUID(asset.OwningCustomerUID);
          log.LogDebug("ProjectIdExecutor: Loaded assetsCustomer? {0}", JsonConvert.SerializeObject(customerAssetOwner));

          assetSubs = dataRepository.LoadAssetSubs(asset.AssetUID, request.timeOfPosition);
          log.LogDebug("ProjectIdExecutor: Loaded assetSubs? {0}", JsonConvert.SerializeObject(assetSubs));
        }
      }

      if (customerTCCOrg != null || customerAssetOwner != null)
      {
        //Look for projects with (request.latitude, request.longitude) inside their boundary
        //and belonging to customers who have a Project Monitoring subscription
        //for asset with id request.assetId at time request.timeOfPosition 
        //and the customer owns the asset. (In VL multiple customers can have subscriptions
        //for an asset but only the owner gets the tag file data).

        ProjectRepository projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;

        //  standard 2d / 3d project aka construction project
        //    IGNORE any tccOrgID
        //    must have valid assetID, which must have a 3d sub.
        if (customerAssetOwner != null && assetSubs != null && assetSubs.Count() > 0)
        {
          var p = projectRepo.GetStandardProject(customerAssetOwner.CustomerUID, request.latitude, request.longitude, request.timeOfPosition);
          if (p.Result != null && p.Result.Count() > 0)
          {
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
            log.LogDebug("ProjectIdExecutor: Loaded standardProjects which lat/long is within {0}", JsonConvert.SerializeObject(p.Result));
          }
          else
          {
            log.LogDebug("ProjectIdExecutor: No standardProjects loaded");
          }
        }

        // ProjectMonitoring project
        //  MUST have a TCCOrgID
        //  customersOrgID must have a PM sub
        //  allow johnDoe assets(-1) and valid assetIDs(no manually imported tagfile(assetid = -2))
        if (customerTCCOrg != null && request.assetId != -2)
        {
          var p = projectRepo.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
                      request.latitude, request.longitude, request.timeOfPosition,
                      (int) ProjectType.ProjectMonitoring, (serviceTypeMappings.serviceTypes.Find(st => st.name == "Project Monitoring").NGEnum));
          if (p.Result != null && p.Result.Count() > 0)
          {
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
            log.LogDebug("ProjectIdExecutor: Loaded pmProjects which lat/long is within {0}", JsonConvert.SerializeObject(p.Result));
          }
          else
          {
            log.LogDebug("ProjectIdExecutor: No pmProjects loaded");
          }
        }

        // Landfill project
        //   MUST have a TCCOrgID
        //   customersOrgID must have a PM sub
        //   allow manual assets(-2) and johnDoe assets(-1) and valid assetIDs
        if (customerTCCOrg != null)
        {
          var p = projectRepo.GetProjectMonitoringProject(customerTCCOrg.CustomerUID,
          request.latitude, request.longitude, request.timeOfPosition,
          (int)ProjectType.LandFill, (serviceTypeMappings.serviceTypes.Find(st => st.name == "Landfill").NGEnum));
          if (p.Result != null && p.Result.Count() > 0)
          { 
            potentialProjects = potentialProjects == null ? p.Result : potentialProjects.Concat(p.Result);
            log.LogDebug("ProjectIdExecutor: Loaded landfillProjects which lat/long is within {0}", JsonConvert.SerializeObject(p.Result));
          }
          else
          {
            log.LogDebug("ProjectIdExecutor: No landfillProjects loaded");
          }
        }

        //projectId
        //If zero found then returns -1
        //If one found then returns its id
        //If > 1 found then returns -2
        if (potentialProjects == null || potentialProjects.Count() == 0)
          projectId = -1;
        else
          if (potentialProjects.Distinct().Count() > 1)
          projectId = -2;
        else
          projectId = potentialProjects.ToList()[0].LegacyProjectID;
        log.LogDebug("ProjectIdExecutor: returning potential projectId {0}", projectId);
      }

      var result = projectId > 1;

      try
      {
        return GetProjectIdResult.CreateGetProjectIdResult(result, projectId);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project id"));
      }

    }
  }
}
