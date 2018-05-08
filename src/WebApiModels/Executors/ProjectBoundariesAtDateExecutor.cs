using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  /// The executor which gets a list of project boundaries for the requested asset id.
  /// </summary>
  public class ProjectBoundariesAtDateExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the get project boundaries request and finds active projects of the asset owner at the given date time.
    /// </summary>
    /// <returns>a GetProjectBoundariesAtDateResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectBoundariesAtDateRequest;
      log.LogDebug("ProjectBoundariesAtDateExecutor: Going to process request {0}",
        JsonConvert.SerializeObject(request));

      var result = false;
      var boundaries = new ProjectBoundaryPackage[0];

      var asset = await dataRepository.LoadAsset(request.assetId);
      log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded Asset? {0}", JsonConvert.SerializeObject(asset));

      if (asset != null)
      {
        // loading customer checks that it is of the correct type
        var assetOwningCustomer = await dataRepository.LoadCustomer(asset.OwningCustomerUID);
        log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsCustomer? {0}",
          JsonConvert.SerializeObject(assetOwningCustomer));

        if (assetOwningCustomer != null)
        {
          // See if the assets owner has a) a 3dPM sub on the asset or b) a Man3d sub       
          // i.e. will be 0 or 1 customer
          var superSubs = await dataRepository.LoadAssetSubs(asset.AssetUID, request.tagFileUTC.Date);
          var subs = superSubs.Where(x => x.customerUid == assetOwningCustomer.CustomerUID);
          log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsSubs? {0}", JsonConvert.SerializeObject(subs));

          // we need only 1 sub.
          var subscriptions = subs as IList<Subscriptions> ?? subs.ToList();
          if (!subscriptions.Any())
          {
            subs = await dataRepository.LoadManual3DCustomerBasedSubs(assetOwningCustomer.CustomerUID,
              request.tagFileUTC.Date);
            subscriptions = subs as IList<Subscriptions> ?? subs.ToList();
            log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theCustomersSubs? {0}",
              JsonConvert.SerializeObject(subs));
          }

          if (subscriptions.Count > 0)
          {
            //Look for projects which are active at date time request.tagFileUTC
            //i.e. tagFileUTC is between project start and end dates
            //and which belong to the asset owner and get their boundary points
            var projects =
              await dataRepository.LoadProjects(asset.OwningCustomerUID, request.tagFileUTC.Date);

            log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded Projects {0} for customer",
              JsonConvert.SerializeObject(projects));

            var enumerable = projects as IList<Project> ?? projects.ToList();
            if (enumerable.Any())
            {
              var list = new List<ProjectBoundaryPackage>();
              foreach (var p in enumerable)
              {
                var boundaryFence =
                  new TWGS84FenceContainer {FencePoints = dataRepository.ParseBoundaryData(p.GeometryWKT)};
                if (boundaryFence.FencePoints.Length > 0)
                {
                  list.Add(new ProjectBoundaryPackage
                  {
                    ProjectID = p.LegacyProjectID,
                    Boundary = boundaryFence
                  });
                }
              }
              boundaries = list.ToArray();
              log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded boundaries {0} for projects",
                JsonConvert.SerializeObject(boundaries));

              if (boundaries.Length > 0) result = true;
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
        throw new ServiceException(HttpStatusCode.InternalServerError,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
            ContractExecutionStatesEnum.InternalProcessingError, 16));
      }
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
