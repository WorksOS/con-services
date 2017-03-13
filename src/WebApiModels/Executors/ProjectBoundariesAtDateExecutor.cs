using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.DBModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
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

      bool result = false;
      ProjectBoundaryPackage[] boundaries = null;

      var asset = LoadAsset(request.assetId);

      if (asset != null)
      {
        //Find the dealer and/or customers who have current 3d sub on this asset and is the Owner of the asset 
        //Find the dealer and/or customers who have current Man3d sub on this asset and is the Owner of the asset 
        // i.e. will be 0 or 1 customer
        IEnumerable<SubscriptionData> subs = null;
        subs = LoadAssetSubs(asset.AssetUID, request.tagFileUTC.Date);
        if (subs == null)
        {
          subs = LoadManual3DCustomerBasedSubs(asset.OwningCustomerUID);
        }
        log.LogDebug("ProjectBoundariesAtDateExecutor: Retrieved Asset and  customer subs {0} for OwningCustomerUID {1}", JsonConvert.SerializeObject(subs), asset.OwningCustomerUID);

        if (subs != null)
        {
          //Look for projects which are active at date time request.tagFileUTC
          //i.e. tagFileUTC is between project start and end dates
          //and which belong to the asset owner and get their boundary points
          
          var g = LoadProjects(asset.OwningCustomerUID, request.tagFileUTC.Date);
          if (g.Result != null)
          {
            IEnumerable<Project> projects = g.Result;
            result = true;
            boundaries = new ProjectBoundaryPackage[projects.Count()];
            for (int i = 0; i < projects.Count(); i++)
            {
              var points = ParseBoundaryData(projects.ElementAt(i).GeometryWKT);
              //boundaries[i] = new TWGS84FenceContainer()
              //{
              //  FencePoints = new TWGS84Point[points.Count()] { points.ToArray() }
              //};
              //boundaries[i].ProjectID = 726;
            }
          }
        }
      }
      else { boundaries = new ProjectBoundaryPackage[0]; }

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

    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}