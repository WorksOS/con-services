using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.DBModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WebApiModels.Models;
using WebApiModels.ResultHandling;

namespace WebApiModels.Executors
{
  /// <summary>
  /// The executor which gets a list of project boundaries for the requested asset id.
  /// </summary>
  public class ProjectBoundariesAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundaries request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundariesAtDateResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectBoundariesAtDateRequest request = item as GetProjectBoundariesAtDateRequest;
      log.LogDebug("ProjectBoundariesAtDateExecutor: Going to process request {0}", JsonConvert.SerializeObject(request));

      bool result = false;
      ProjectBoundaryPackage[] boundaries = new ProjectBoundaryPackage[0];

      var asset = LoadAsset(request.assetId);
      log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded Asset? {0}", JsonConvert.SerializeObject(asset));

      if (asset != null)
      {
        // loading customer checks that it is of the correct type
        Customer assetOwningCustomer = LoadCustomer(asset.OwningCustomerUID);
        log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsCustomer? {0}", JsonConvert.SerializeObject(assetOwningCustomer));

        if (assetOwningCustomer != null)
        {
          // See if the assets owner has a) a 3dPM sub on the asset or b) a Man3d sub       
          // i.e. will be 0 or 1 customer
          IEnumerable<SubscriptionData> subs = null;
          subs = LoadAssetSubs(asset.AssetUID, request.tagFileUTC.Date)
                .Where(x => x.customerUid == assetOwningCustomer.CustomerUID);
          log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsSubs? {0}", JsonConvert.SerializeObject(subs));

          // we need only 1 sub.
          if (subs == null || subs.Count() == 0)
          {
            subs = LoadManual3DCustomerBasedSubs(assetOwningCustomer.CustomerUID, request.tagFileUTC.Date);
            log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theCustomersSubs? {0}", JsonConvert.SerializeObject(subs));
          }
          
          if (subs != null && subs.Count() > 0)
          {
            //Look for projects which are active at date time request.tagFileUTC
            //i.e. tagFileUTC is between project start and end dates
            //and which belong to the asset owner and get their boundary points

            IEnumerable<Project> projects = LoadProjects(asset.OwningCustomerUID, request.tagFileUTC.Date);
            log.LogInformation("ProjectBoundariesAtDateExecutor: Loaded Projects {0} for customer", JsonConvert.SerializeObject(projects));

            if (projects != null && projects.Count() > 0)
            {
              List<ProjectBoundaryPackage> list = new List<ProjectBoundaryPackage>();
              foreach (var p in projects)
              {
                TWGS84FenceContainer boundaryFence = new TWGS84FenceContainer();
                boundaryFence.FencePoints = ParseBoundaryData(p.GeometryWKT);
                if (boundaryFence.FencePoints.Count() > 0)
                {
                  list.Add(new ProjectBoundaryPackage
                  {
                    ProjectID = p.LegacyProjectID,
                    Boundary = boundaryFence
                  });
                }
              }
              boundaries = list.ToArray();
              log.LogInformation("ProjectBoundariesAtDateExecutor: Loaded boundaries {0} for projects", JsonConvert.SerializeObject(boundaries));

              if (boundaries.Count() > 0) result = true;
            }
          }
        }
      }      

      try
      {
        return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(result, boundaries);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project boundaries"));
      }
    }    
  }
}