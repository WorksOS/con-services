﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a GetProjectBoundariesAtDateResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      GetProjectBoundariesAtDateRequest request = item as GetProjectBoundariesAtDateRequest;
      log.LogDebug("ProjectBoundariesAtDateExecutor: Going to process request {0}",
        JsonConvert.SerializeObject(request));

      bool result = false;
      ProjectBoundaryPackage[] boundaries = new ProjectBoundaryPackage[0];

      var asset = dataRepository.LoadAsset(request.assetId);
      log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded Asset? {0}", JsonConvert.SerializeObject(asset));

      if (asset != null)
      {
        // loading customer checks that it is of the correct type
        Customer assetOwningCustomer = dataRepository.LoadCustomer(asset.OwningCustomerUID);
        log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsCustomer? {0}",
          JsonConvert.SerializeObject(assetOwningCustomer));

        if (assetOwningCustomer != null)
        {
          // See if the assets owner has a) a 3dPM sub on the asset or b) a Man3d sub       
          // i.e. will be 0 or 1 customer
          IEnumerable<Subscriptions> subs = null;
          subs = dataRepository.LoadAssetSubs(asset.AssetUID, request.tagFileUTC.Date)
            .Where(x => x.customerUid == assetOwningCustomer.CustomerUID);
          log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theAssetsSubs? {0}", JsonConvert.SerializeObject(subs));

          // we need only 1 sub.
          if (subs == null || subs.Count() == 0)
          {
            subs = dataRepository.LoadManual3DCustomerBasedSubs(assetOwningCustomer.CustomerUID,
              request.tagFileUTC.Date);
            log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded theCustomersSubs? {0}",
              JsonConvert.SerializeObject(subs));
          }

          if (subs != null && subs.Count() > 0)
          {
            //Look for projects which are active at date time request.tagFileUTC
            //i.e. tagFileUTC is between project start and end dates
            //and which belong to the asset owner and get their boundary points

            IEnumerable<Project> projects =
              dataRepository.LoadProjects(asset.OwningCustomerUID, request.tagFileUTC.Date);
            log.LogDebug("ProjectBoundariesAtDateExecutor: Loaded Projects {0} for customer",
              JsonConvert.SerializeObject(projects));

            if (projects != null && projects.Any())
            {
              List<ProjectBoundaryPackage> list = new List<ProjectBoundaryPackage>();
              foreach (var p in projects)
              {
                TWGS84FenceContainer boundaryFence =
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
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
            ContractExecutionStatesEnum.InternalProcessingError, "Failed to get project boundaries"));
      }
    }
  }
}