using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

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

      var device = await dataRepository.GetDevice(request.shortRaptorAssetId);
      log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}: Loaded Device? {JsonConvert.SerializeObject(device)}");

      var deviceLicenseTotal = 0;
      if (device != null)
      {
        if (device.Status != "Claimed")
          log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}: Device is not registered and claimed");
        else
          deviceLicenseTotal = await dataRepository.GetDeviceLicenses(device.CustomerUID);
      }

      if (device == null || deviceLicenseTotal < 1)
        return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(result, projectBoundaryPackages);

      // claimed device and device owner account has valid licenses

      //Look for projects which are active at date time request.tagFileUTC
      //i.e. tagFileUTC is between project start and end dates
      //and which belong to the asset owner and get their boundary points
      var projects = await dataRepository.GetProjects(device.CustomerUID, request.tagFileUTC.Date);
      log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded Projects for customer {JsonConvert.SerializeObject(projects)}");

      if (projects.Any())
      {
        foreach (var p in projects)
        {
          var boundaryFence = new TWGS84FenceContainer {FencePoints = dataRepository.ParseBoundaryData(p.GeometryWKT)};
          if (boundaryFence.FencePoints.Length > 0)
          {
            projectBoundaryPackages.Add(new ProjectBoundaryPackage {ProjectID = p.ShortRaptorProjectId, Boundary = boundaryFence});
          }
        }

        log.LogDebug($"{nameof(ProjectBoundariesAtDateExecutor)}:  Loaded boundaries for projects {JsonConvert.SerializeObject(projectBoundaryPackages)}");

        if (projectBoundaryPackages.Count > 0) result = true;
      }

      return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(result, projectBoundaryPackages);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
