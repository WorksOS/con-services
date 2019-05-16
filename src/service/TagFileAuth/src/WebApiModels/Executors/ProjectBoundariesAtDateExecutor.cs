using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
    /// <returns>a GetProjectBoundariesAtDateResult if successful</returns>      
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as GetProjectBoundariesAtDateRequest;
      var result = false;
      var projectBoundaryPackages = new List<ProjectBoundaryPackage>();

      var asset = await dataRepository.LoadAsset(request.assetId);
      log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}: Loaded Asset? {JsonConvert.SerializeObject(asset)}");

      if (asset != null)
      {
        // loading customer checks that it is of the correct type
        var assetOwningCustomer = await dataRepository.LoadCustomer(asset.OwningCustomerUID);
        log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded assetOwningCustomer? {JsonConvert.SerializeObject(assetOwningCustomer)}");

        if (assetOwningCustomer != null)
        {
          // See if the assets owner has a) a 3dPM sub on the asset or b) a Man3d sub       
          // i.e. will be 0 or 1 customer
          var assetSubs = await dataRepository.LoadAssetSubs(asset.AssetUID, request.tagFileUTC.Date);
          var assetSubsForOwningCustomer = assetSubs.FindAll(x => x.customerUid == assetOwningCustomer.CustomerUID);
          log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded assetSubsForOwningCustomer? {JsonConvert.SerializeObject(assetSubsForOwningCustomer)}");

          // we need only 1 sub.
          if (!assetSubsForOwningCustomer.Any())
          {
            var assetManualSubsForOwningCustomer = await dataRepository.LoadManual3DCustomerBasedSubs(assetOwningCustomer.CustomerUID, request.tagFileUTC.Date);
            assetSubsForOwningCustomer.AddRange(assetManualSubsForOwningCustomer);
            log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded assetManualSubsForOwningCustomer? {JsonConvert.SerializeObject(assetManualSubsForOwningCustomer)}");
          }

          if (assetSubsForOwningCustomer.Any())
          {
            //Look for projects which are active at date time request.tagFileUTC
            //i.e. tagFileUTC is between project start and end dates
            //and which belong to the asset owner and get their boundary points
            var projects = await dataRepository.LoadProjects(asset.OwningCustomerUID, request.tagFileUTC.Date);
            log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded Projects for customer {JsonConvert.SerializeObject(projects)}");

            if (projects.Any())
            {
              foreach (var p in projects)
              {
                var boundaryFence = new TWGS84FenceContainer {FencePoints = dataRepository.ParseBoundaryData(p.GeometryWKT)};
                if (boundaryFence.FencePoints.Length > 0)
                {
                  projectBoundaryPackages.Add(new ProjectBoundaryPackage
                  {
                    ProjectID = p.LegacyProjectID,
                    Boundary = boundaryFence
                  });
                }
              }
              log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded boundaries for projects {JsonConvert.SerializeObject(projectBoundaryPackages)}");

              if (projectBoundaryPackages.Count > 0) result = true;
            }
          }
        }
      }

      return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(result, projectBoundaryPackages);
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
