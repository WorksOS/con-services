using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting elevation data from Raptor
  /// </summary>
  //Turn off caching until settings caching problem resolved
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionElevationController : BaseController
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
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="loggerFactory">LoggerFactory</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="elevProxy">Elevation extents proxy</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="exceptionHandler">Service exception handler</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    /// <param name="productionDataRequestFactory"></param>
    public CompactionElevationController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IElevationExtentsProxy elevProxy, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy,
      ICompactionSettingsManager settingsManager, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, 
      IProductionDataRequestFactory productionDataRequestFactory) :
      base(loggerFactory, loggerFactory.CreateLogger<CompactionElevationController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.elevProxy = elevProxy;
      requestFactory = productionDataRequestFactory;
    }

    /// <summary>
    /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectUidVerifier]
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

        ElevationStatisticsResult result = elevProxy.GetElevationRange(GetLegacyProjectId(projectUid), filter, projectSettings);
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
    [ProjectUidVerifier]
    [Route("api/v2/alignmentstationrange")]
    [HttpGet]
    public async Task<AlignmentStationResult> GetAlignmentStationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid alignmentFileUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetAlignmentStationRange: " + Request.QueryString);
      var projectId = GetLegacyProjectId(projectUid);

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
    [ProjectUidVerifier]
    [Route("api/v2/projectstatistics")]
    [HttpGet]
    public async Task<ProjectStatisticsResult> GetProjectStatistics(
      [FromQuery] Guid projectUid)
    {
      Log.LogInformation("GetProjectStatistics: " + Request.QueryString);
      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(GetLegacyProjectId(projectUid), excludedIds?.ToArray());
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
  }
}