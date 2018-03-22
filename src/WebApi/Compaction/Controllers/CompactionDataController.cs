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
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ProjectUidVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionDataController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="exceptionHandler">Service exception handler</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    /// <param name="requestFactory">The request factory.</param>
    public CompactionDataController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IProductionDataRequestFactory requestFactory)
      : base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionDataController>();
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [Route("api/v2/cmv/percentchange")]
    [HttpGet]
    public async Task<CompactionCmvPercentChangeResult> GetCmvPercentChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetCmvPercentChange: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "No CMV Data"));

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await this.GetProjectSettingsTargets(projectUid);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      double[] cmvChangeSummarySettings = this.SettingsManager.CompactionCmvPercentChangeSettings(projectSettings);
      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(
        projectId, null, liftSettings, filter, -1, cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(logger, raptorClient)
          .Process(request) as CMVChangeSummaryResult;
        var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result);
        log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));
        //Short-circuit cache time for Archived projects
        if ((User as RaptorPrincipal).GetProject(projectUid).isArchived)
          Response.Headers["Cache-Control"] = "public,max-age=31536000";
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
        log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [Route("api/v2/cmv/details")]
    [HttpGet]
    public async Task<CompactionCmvDetailedResult> GetCmvDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetCmvDetails: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(logger, raptorClient)
          .Process(request) as CMVDetailedResult;
        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result);

        log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

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
        log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [Route("api/v2/passcounts/details")]
    [HttpGet]
    public async Task<CompactionPassCountDetailedResult> GetPassCountDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetPassCountDetails: " + Request.QueryString);

      PassCounts request = await GetPassCountRequest(projectUid, filterUid, false);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<DetailedPassCountExecutor>(logger, raptorClient)
          .Process(request) as PassCountDetailedResult;
        var returnResult = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(result);
        log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));
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
        log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get cut-fill details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cutfill/details")]
    [HttpGet]
    public async Task<CompactionCutFillDetailedResult> GetCutFillDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid cutfillDesignUid)
    {
      log.LogInformation("GetCutFillDetails: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      var cutFillRequest = requestFactory.Create<CutFillRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
        .CreateCutFillDetailsRequest();

      cutFillRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionCutFillExecutor>(logger, raptorClient)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }

    /// <summary>
    /// Tests if there is overlapping data in Raptor 
    /// </summary>
    private async Task<bool> ValidateFilterAgainstProjectExtents(Guid projectUid, Guid? filterUid)
    {
      log.LogInformation("GetProjectStatistics: " + Request.QueryString);

      //No filter - so proceed further
      if (!filterUid.HasValue)
        return true;

      var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds?.ToArray());
      request.Validate();
      try
      {
        var projectExtents =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
            .Process(request) as ProjectStatisticsResult;

        //No data in Raptor - stop
        if (projectExtents == null)
          return false;

        var filter = await GetCompactionFilter(projectUid, filterUid);

        //No filter dates defined - project extents requested. Proceed further
        if (filter.StartUtc == null && filter.EndUtc == null)
          return true;

        //Do we have intersecting dates? True if yes
        if (filter.StartUtc != null && filter.EndUtc != null)
          return projectExtents.startTime <= filter.EndUtc && filter.StartUtc <= projectExtents.endTime;

        //All other cases - rpoceed further
        return true;
      }
      catch (Exception)
      {
        //Some expcetion - do not proceed further
        return false;
      }
    }

    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <returns>An instance of the CMVRequest class.</returns>
    private async Task<CMVRequest> GetCmvRequest(Guid projectUid, Guid? filterUid)
    {
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      CMVSettings cmvSettings = this.SettingsManager.CompactionCmvSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      return CMVRequest.CreateCMVRequest(projectId, null, cmvSettings, liftSettings, filter, -1, null, null, null);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    /// <param name="projectUid">Project Uid</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="isSummary">True for summary request, false for details request</param>
    /// <returns>An instance of the PassCounts class.</returns>
    private async Task<PassCounts> GetPassCountRequest(Guid projectUid, Guid? filterUid, bool isSummary)
    {
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await this.GetProjectSettingsTargets(projectUid);
      PassCountSettings passCountSettings = isSummary ? null : this.SettingsManager.CompactionPassCountSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      return PassCounts.CreatePassCountsRequest(projectId, null, passCountSettings, liftSettings, filter, -1, null, null, null);
    }
  }
}