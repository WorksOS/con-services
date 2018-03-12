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
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionSummaryDataController : BaseController
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
    public CompactionSummaryDataController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy)
      : base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionDataController>();
    }

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/cmv/summary")]
    [HttpGet]
    public async Task<CompactionCmvSummaryResult> GetCmvSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetCmvSummary: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        return CompactionCmvSummaryResult.CreateCmvSummaryResult(CMVSummaryResult.Empty(), request.cmvSettings);

      log.LogDebug("GetCmvSummary request for Raptor: " + JsonConvert.SerializeObject(request));
      try
      {
        var result =
          RequestExecutorContainerFactory.Build<SummaryCMVExecutor>(logger, raptorClient).Process(request) as
            CMVSummaryResult;
        var returnResult = CompactionCmvSummaryResult.CreateCmvSummaryResult(result, request.cmvSettings);
        log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));

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
        log.LogInformation("GetCmvSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get MDP summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// when the filter layer method is OffsetFromDesign or OffsetFromProfile.
    [ProjectUidVerifier]
    [Route("api/v2/mdp/summary")]
    [HttpGet]
    public async Task<CompactionMdpSummaryResult> GetMdpSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetMdpSummary: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      MDPSettings mdpSettings = this.SettingsManager.CompactionMdpSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        return CompactionMdpSummaryResult.CreateMdpSummaryResult(MDPSummaryResult.Empty(), mdpSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      MDPRequest request = MDPRequest.CreateMDPRequest(projectId, null, mdpSettings, liftSettings, filter,
        -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<SummaryMDPExecutor>(logger, raptorClient, null, this.ConfigStore)
          .Process(request) as MDPSummaryResult;
        var returnResult = CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
        log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(returnResult));
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
        log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/passcounts/summary")]
    [HttpGet]
    public async Task<CompactionPassCountSummaryResult> GetPassCountSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        return CompactionPassCountSummaryResult.CreatePassCountSummaryResult(PassCountSummaryResult.Empty());


      PassCounts request = await GetPassCountRequest(projectUid, filterUid, true);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(logger, raptorClient)
          .Process(request) as PassCountSummaryResult;
        var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
        log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));
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
        log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/temperature/summary")]
    [HttpGet]
    public async Task<CompactionTemperatureSummaryResult> GetTemperatureSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetTemperatureSummary: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        return CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(TemperatureSummaryResult.Empty());

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      TemperatureSettings temperatureSettings = this.SettingsManager.CompactionTemperatureSettings(projectSettings, false);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(projectId, null,
        temperatureSettings, liftSettings, filter, -1, null, null, null);
      request.Validate();
      try
      {
        var result =
          RequestExecutorContainerFactory.Build<SummaryTemperatureExecutor>(logger, raptorClient)
            .Process(request) as TemperatureSummaryResult;
        var returnResult = CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
        log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(returnResult));
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
        log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/speed/summary")]
    [HttpGet]
    public async Task<CompactionSpeedSummaryResult> GetSpeedSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetSpeedSummary: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      //Speed settings are in LiftBuildSettings
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
        return CompactionSpeedSummaryResult.CreateSpeedSummaryResult(SummarySpeedResult.Empty(), liftSettings.machineSpeedTarget);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      SummarySpeedRequest request =
        SummarySpeedRequest.CreateSummarySpeedRequest(projectId, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<SummarySpeedExecutor>(logger, raptorClient)
          .Process(request) as SummarySpeedResult;
        var returnResult =
          CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result, liftSettings.machineSpeedTarget);
        log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(returnResult));
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
        log.LogInformation("GetSpeedSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="volumeSummaryHelper">Volume Summary helper.</param>
    /// <param name="projectUid">The project Uid.</param>
    /// <param name="baseUid">The Uid for the base surface, either a filter or design.</param>
    /// <param name="topUid">The Uid for the top surface, either a filter or design.</param>
    [ProjectUidVerifier]
    [Route("api/v2/volumes/summary")]
    [HttpGet]
    public async Task<CompactionSummaryVolumesResult> GetSummaryVolumes(
      [FromServices] IVolumeSummaryHelper volumeSummaryHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid baseUid,
      [FromQuery] Guid topUid)
    {
      log.LogInformation("GetSummaryVolumes: " + Request.QueryString);

      if (baseUid == Guid.Empty || topUid == Guid.Empty)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid surface parameter(s)."));
      }

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);

      DesignDescriptor baseDesign = null;
      DesignDescriptor topDesign = null;
      Filter baseFilter = null;
      Filter topFilter = null;

      var baseFilterDescriptor = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, baseUid));

      if (baseFilterDescriptor == null)
      {
        baseDesign = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, baseUid));
      }
      else
      {
        baseFilter = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, baseUid));
      }

      var topFilterDescriptor = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, topUid));
      if (topFilterDescriptor == null)
      {
        topDesign = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, topUid));
      }
      else
      {
        topFilter = await volumeSummaryHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, topUid));
      }

      if (baseFilter == null && baseDesign == null || topFilter == null && topDesign == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Can not resolve either baseSurface or topSurface"));
      }

      var volumeCalcType = volumeSummaryHelper.GetVolumesType(baseFilter, topFilter);
      var request = SummaryVolumesRequest.CreateAndValidate(projectId, baseFilter, topFilter, baseDesign, topDesign, volumeCalcType);

      CompactionSummaryVolumesResult returnResult;

      try
      {
        var result = RequestExecutorContainerFactory
          .Build<SummaryVolumesExecutorV2>(logger, raptorClient)
          .Process(request) as SummaryVolumesResult;

        returnResult = CompactionSummaryVolumesResult.CreateInstance(result, await GetProjectSettingsTargets(projectUid));
      }
      catch (ServiceException)
      {
        returnResult = CompactionSummaryVolumesResult.CreateInstance(
          SummaryVolumesResult.CreateEmptySummaryVolumesResult(),
          await GetProjectSettingsTargets(projectUid));
      }
      finally
      {
        log.LogInformation("GetSummaryVolumes returned: " + Response.StatusCode);
      }

      log.LogTrace("GetSummaryVolumes result: " + JsonConvert.SerializeObject(returnResult));

      return returnResult;
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