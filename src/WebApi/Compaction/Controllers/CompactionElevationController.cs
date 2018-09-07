using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting elevation data from Raptor
  /// </summary>
  //Turn off caching until settings caching problem resolved
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionElevationController : BaseController<CompactionElevationController>
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionElevationController(IASNodeClient raptorClient, IConfigurationStore configStore, IElevationExtentsProxy elevProxy, IFileListProxy fileListProxy,
      ICompactionSettingsManager settingsManager, IProductionDataRequestFactory productionDataRequestFactory) :
      base(configStore, fileListProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.elevProxy = elevProxy;
      requestFactory = productionDataRequestFactory;
    }

    /// <summary>
    /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/elevationrange")]
    [HttpGet]
    public async Task<ElevationStatisticsResult> GetElevationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetElevationRange: " + Request.QueryString);

      try
      {
        var projectSettings = await GetProjectSettingsTargets(projectUid);

        var filter = await GetCompactionFilter(projectUid, filterUid);
        var projectId = await GetLegacyProjectId(projectUid);
        ElevationStatisticsResult result = elevProxy.GetElevationRange(projectId, filter, projectSettings);
        if (result == null)
        {
          //Ideally want to return an error code and message only here
          result = ElevationStatisticsResult.CreateElevationStatisticsResult(null, 0, 0, 0);
        }
        Log.LogInformation("GetElevationRange result: " + JsonConvert.SerializeObject(result));
        return result;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      finally
      {
        Log.LogInformation("GetElevationRange returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Gets alignment file extents (station range) from Raptor.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="alignmentFileUid">Alignment file UID</param>
    /// <param name="boundingBoxService">Project UID</param>
    /// <returns>Station range for alignment file</returns>
    [ProjectVerifier]
    [Route("api/v2/alignmentstationrange")]
    [HttpGet]
    public async Task<AlignmentStationResult> GetAlignmentStationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid alignmentFileUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetAlignmentStationRange: " + Request.QueryString);
      var projectId = await GetLegacyProjectId(projectUid);

      var alignmentDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentFileUid);

      var request = requestFactory.Create<AlignmentStationRangeRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(CustomHeaders))
        .CreateAlignmentStationRangeRequest(alignmentDescriptor);

      request.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        boundingBoxService.GetAlignmentStationRange(projectId, alignmentDescriptor));

      return result;
    }

    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <returns>Project statistics</returns>
    [ProjectVerifier]
    [Route("api/v2/projectstatistics")]
    [HttpGet]
    public async Task<ProjectStatisticsResult> GetProjectStatistics(
      [FromQuery] Guid projectUid)
    {
      Log.LogInformation("GetProjectStatistics: " + Request.QueryString);
      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      var projectId = await GetLegacyProjectId(projectUid);

      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds?.ToArray());
      request.Validate();
      try
      {
        var returnResult =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(LoggerFactory, raptorClient)
            .Process(request) as ProjectStatisticsResult;
        Log.LogInformation("GetProjectStatistics result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      finally
      {
        Log.LogInformation("GetProjectStatistics returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Gets production data extents from Raptor in lat/lng degrees.
    /// </summary>
    /// <returns>Production data extents</returns>
    [ProjectVerifier]
    [Route("api/v2/productiondataextents")]
    [HttpGet]
    public async Task<ProjectExtentsResult> GetProjectExtentsV2(
      [FromQuery] Guid projectUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetProjectExtents V2: " + Request.QueryString);
      var projectId = await GetLegacyProjectId(projectUid);
      return await GetProjectExtentsV1(projectId, boundingBoxService);
    }

    /// <summary>
    /// Gets production data extents from Raptor in lat/lng degrees.
    /// </summary>
    /// <returns>Production data extents</returns>
    [ProjectVerifier]
    [Route("api/v1/productiondataextents")]
    [HttpGet]
    public async Task<ProjectExtentsResult> GetProjectExtentsV1(
      [FromQuery] long projectId,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetProjectExtents V1: " + Request.QueryString);
      var project = await ((RaptorPrincipal) User).GetProject(projectId);
      var excludedIds = await GetExcludedSurveyedSurfaceIds(Guid.Parse(project.ProjectUid));

      try
      {
        var result = boundingBoxService.GetProductionDataExtents(projectId, excludedIds);

        var returnResult = new ProjectExtentsResult
        {
          minLat = result.conversionCoordinates[0].y.LatRadiansToDegrees(),
          minLng = result.conversionCoordinates[0].x.LonRadiansToDegrees(),
          maxLat = result.conversionCoordinates[1].y.LatRadiansToDegrees(),
          maxLng = result.conversionCoordinates[1].x.LonRadiansToDegrees()
        };

        Log.LogInformation("GetProjectExtents result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      finally
      {
        Log.LogInformation("GetProjectExtents returned: " + Response.StatusCode);
      }
    }
  }
}
