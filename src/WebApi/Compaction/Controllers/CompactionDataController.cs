using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionDataController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// 
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


    #region Summary Data for Widgets

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>CMV summary</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/summary")]
    [HttpGet]
    public async Task<CompactionCmvSummaryResult> GetCmvSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetCmvSummary: " + Request.QueryString);
      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();
      log.LogDebug("GetCmvSummary request for Raptor: " + JsonConvert.SerializeObject(request));
      try
      {
        var result =
          RequestExecutorContainerFactory.Build<SummaryCMVExecutor>(logger, raptorClient).Process(request) as
            CMVSummaryResult;
        var returnResult = CompactionCmvSummaryResult.CreateCmvSummaryResult(result, request.cmvSettings);
        log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));
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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// when the filter layer method is OffsetFromDesign or OffsetFromProfile.
    /// <returns>MDP summary</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/mdp/summary")]
    [HttpGet]
    public async Task<CompactionMdpSummaryResult> GetMdpSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetMdpSummary: " + Request.QueryString);
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      MDPSettings mdpSettings = this.SettingsManager.CompactionMdpSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>Pass count summary</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/summary")]
    [HttpGet]
    public async Task<CompactionPassCountSummaryResult> GetPassCountSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      PassCounts request = await GetPassCountRequest(projectUid, filterUid, true);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(logger, raptorClient)
          .Process(request) as PassCountSummaryResult;
        var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
        log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));
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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>Temperature summary</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/temperature/summary")]
    [HttpGet]
    public async Task<CompactionTemperatureSummaryResult> GetTemperatureSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetTemperatureSummary: " + Request.QueryString);
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>Speed summary</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/speed/summary")]
    [HttpGet]
    public async Task<CompactionSpeedSummaryResult> GetSpeedSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetSpeedSummary: " + Request.QueryString);
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
      //Speed settings are in LiftBuildSettings
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

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
    /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>CMV % change</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/percentchange")]
    [HttpGet]
    public async Task<CompactionCmvPercentChangeResult> GetCmvPercentChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      log.LogInformation("GetCmvPercentChange: " + Request.QueryString);
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await this.GetProjectSettings(projectUid);
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

    private T WithSwallowExceptionExecute<T>(Func<T> a) where T : class
    {
      try
      {
        return a.Invoke();
      }
      catch
      {}
      return default(T);
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="volumeSummaryHelper">Volume Summary helper.</param>
    /// <param name="projectUid">The project Uid.</param>
    /// <param name="baseUid">The Uid for the base surface, either a filter or design.</param>
    /// <param name="topUid">The Uid for the top surface, either a filter or design.</param>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/volumes/summary")]
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
      MasterData.Models.Models.Filter baseFilterDescriptor = null;
      MasterData.Models.Models.Filter topFilterDescriptor = null;


      baseFilterDescriptor = await WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, baseUid));

      if (baseFilterDescriptor == null)
        {
          baseDesign = await WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, baseUid));
        }
        else
        {
          baseFilter = await WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, baseUid));
          // TODO Validate is not null.
        }

        topFilterDescriptor = await WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, topUid));
        if (topFilterDescriptor == null)
        {
          topDesign = await WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, topUid));
        }
        else
        {
          topFilter = await WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, topUid));
          // TODO Validate is not null.
        }

      if ((baseFilter == null && baseDesign == null) || (topFilter == null && topDesign == null))
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Can not resolve either baseUsrface or topSurface"));

      // Ground to Ground
      if (baseFilterDescriptor != null && topFilterDescriptor != null)
      {
        if (baseFilterDescriptor.DateRangeType != DateRangeType.ProjectExtents &&
            baseFilterDescriptor.DateRangeType == topFilterDescriptor.DateRangeType &&
            baseFilterDescriptor.elevationType == null && topFilterDescriptor.elevationType == null)
        {
          // TODO (Aaron) We need to review and understand v1 workflow around setting 'earliest'. This will be delivered in a second patch.
        }
        else
        {
          // Fall through and use the previously setup filters.
        }
      }

      var volumeCalcType = volumeSummaryHelper.GetVolumesType(baseFilter, topFilter);
      var request = SummaryVolumesRequest.CreateAndValidate(projectId, baseFilter, topFilter, baseDesign, topDesign, volumeCalcType);

      CompactionSummaryVolumesResult returnResult;

      try
      {
        var result = RequestExecutorContainerFactory
          .Build<SummaryVolumesExecutor>(logger, raptorClient)
          .Process(request) as SummaryVolumesResult;

        returnResult = CompactionSummaryVolumesResult.CreateInstance(result, await GetProjectSettings(projectUid));
      }
      catch (ServiceException)
      {
        returnResult = CompactionSummaryVolumesResult.CreateInstance(
          SummaryVolumesResult.CreateEmptySummaryVolumesResult(),
          await GetProjectSettings(projectUid));
      }
      finally
      {
        log.LogInformation("GetSummaryVolumes returned: " + Response.StatusCode);
      }

      log.LogTrace("GetSummaryVolumes result: " + JsonConvert.SerializeObject(returnResult));

      return returnResult;
    }

    #endregion

    #region Detailed Data for the map

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>CMV details</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/details")]
    [HttpGet]
    public async Task<CompactionCmvDetailedResult> GetCmvDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      /**************************************************************************************************
       * NOTE: This end point for CMV details is currently not called from the Compaction UI.
       * It still uses the old Raptor CMV settings with CMV min, max and target to calculate the percents
       * data to return. However, the palette now uses 16 colors and values (the last being 'above' color
       * and value) and this code needs to be updated to be consistent. This requires a change to Raptor
       * to accept a list of CMV values like for pass count details.
       **************************************************************************************************/

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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>Pass count details</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/details")]
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
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>Cut-fill details</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cutfill/details")]
    [HttpGet]
    public async Task<CompactionCutFillDetailedResult> GetCutFillDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid cutfillDesignUid)
    {
      log.LogInformation("GetCutFillDetails: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
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
    #endregion

    #region privates
    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <param name="projectUid">Project Uid</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>An instance of the CMVRequest class.</returns>
    private async Task<CMVRequest> GetCmvRequest(Guid projectUid, Guid? filterUid)
    {
      var projectId = ((RaptorPrincipal)this.User).GetProjectId(projectUid);
      var projectSettings = await GetProjectSettings(projectUid);
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
      var projectSettings = await this.GetProjectSettings(projectUid);
      PassCountSettings passCountSettings = isSummary ? null : this.SettingsManager.CompactionPassCountSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      return PassCounts.CreatePassCountsRequest(projectId, null, passCountSettings, liftSettings, filter, -1, null, null, null);
    }

    #endregion
  }
}
