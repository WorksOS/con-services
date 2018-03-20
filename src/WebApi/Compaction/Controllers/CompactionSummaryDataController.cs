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
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary data requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionSummaryDataController : CompactionDataBaseController
  {
    /// <inheritdoc />
    public CompactionSummaryDataController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore,
                                           IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
                                           IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IProductionDataRequestFactory requestFactory)
      : base(raptorClient, loggerFactory, loggerFactory.CreateLogger<CompactionSummaryDataController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager, requestFactory)
    { }

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/cmv/summary")]
    [HttpGet]
    public async Task<CompactionCmvSummaryResult> GetCmvSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvSummary: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionCmvSummaryResult.CreateEmptyResult();
      }

      Log.LogDebug("GetCmvSummary request for Raptor: " + JsonConvert.SerializeObject(request));
      try
      {
        var result = RequestExecutorContainerFactory
              .Build<SummaryCMVExecutor>(LoggerFactory, RaptorClient)
              .Process(request) as CMVSummaryResult;

        var returnResult = CompactionCmvSummaryResult.Create(result, request.cmvSettings);
        Log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));

        SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetCmvSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get MDP summary from Raptor for the specified project and date range.
    /// </summary>
    /// when the filter layer method is OffsetFromDesign or OffsetFromProfile.
    [ProjectUidVerifier]
    [Route("api/v2/mdp/summary")]
    [HttpGet]
    public async Task<CompactionMdpSummaryResult> GetMdpSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetMdpSummary: " + Request.QueryString);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      MDPSettings mdpSettings = this.SettingsManager.CompactionMdpSettings(projectSettings);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionMdpSummaryResult.CreateEmptyResult();
      }

      var filter = await GetCompactionFilter(projectUid, filterUid);

      MDPRequest request = MDPRequest.CreateMDPRequest(GetLegacyProjectId(projectUid), null, mdpSettings, liftSettings, filter,
        -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<SummaryMDPExecutor>(LoggerFactory, RaptorClient, null, this.ConfigStore)
          .Process(request) as MDPSummaryResult;
        var returnResult = CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
        Log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(returnResult));

        SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/passcounts/summary")]
    [HttpGet]
    public async Task<CompactionPassCountSummaryResult> GetPassCountSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionPassCountSummaryResult.CreateEmptyResult();
      }

      PassCounts request = await GetPassCountRequest(projectUid, filterUid, true);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<SummaryPassCountsExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as PassCountSummaryResult;
        var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
        Log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));

        SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/temperature/summary")]
    [HttpGet]
    public async Task<CompactionTemperatureSummaryResult> GetTemperatureSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetTemperatureSummary: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionTemperatureSummaryResult.CreateEmptyResult();
      }

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      TemperatureSettings temperatureSettings = this.SettingsManager.CompactionTemperatureSettings(projectSettings, false);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(GetLegacyProjectId(projectUid), null,
        temperatureSettings, liftSettings, filter, -1, null, null, null);
      request.Validate();
      try
      {
        var result =
          RequestExecutorContainerFactory.Build<SummaryTemperatureExecutor>(LoggerFactory, RaptorClient)
            .Process(request) as TemperatureSummaryResult;
        var returnResult = CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
        Log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(returnResult));

        SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
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
      Log.LogInformation("GetSpeedSummary: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      //Speed settings are in LiftBuildSettings
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionSpeedSummaryResult.CreateEmptyResult();
      }

      var filter = await GetCompactionFilter(projectUid, filterUid);

      SummarySpeedRequest request =
        SummarySpeedRequest.CreateSummarySpeedRequest(projectId, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<SummarySpeedExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as SpeedSummaryResult;
        var returnResult =
          CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result, liftSettings.machineSpeedTarget);
        Log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(returnResult));

        SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetSpeedSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="summaryDataHelper">Volume Summary helper.</param>
    /// <param name="projectUid">The project Uid.</param>
    /// <param name="baseUid">The Uid for the base surface, either a filter or design.</param>
    /// <param name="topUid">The Uid for the top surface, either a filter or design.</param>
    [ProjectUidVerifier]
    [Route("api/v2/volumes/summary")]
    [HttpGet]
    public async Task<CompactionVolumesSummaryResult> GetSummaryVolumes(
      [FromServices] ISummaryDataHelper summaryDataHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid baseUid,
      [FromQuery] Guid topUid)
    {
      Log.LogInformation("GetSummaryVolumes: " + Request.QueryString);

      if (baseUid == Guid.Empty || topUid == Guid.Empty)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid surface parameter(s)."));
      }

      DesignDescriptor baseDesign = null;
      DesignDescriptor topDesign = null;
      Filter baseFilter = null;
      Filter topFilter = null;

      var baseFilterDescriptor = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, baseUid));

      if (baseFilterDescriptor == null)
      {
        baseDesign = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, baseUid));
      }
      else
      {
        baseFilter = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, baseUid));
      }

      var topFilterDescriptor = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetFilterDescriptor(projectUid, topUid));
      if (topFilterDescriptor == null)
      {
        topDesign = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetAndValidateDesignDescriptor(projectUid, topUid));
      }
      else
      {
        topFilter = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, topUid));
      }

      if (baseFilter == null && baseDesign == null || topFilter == null && topDesign == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Can not resolve either baseSurface or topSurface"));
      }

      var volumeCalcType = summaryDataHelper.GetVolumesType(baseFilter, topFilter);
      var request = SummaryVolumesRequest.CreateAndValidate(GetLegacyProjectId(projectUid), baseFilter, topFilter, baseDesign, topDesign, volumeCalcType);

      CompactionVolumesSummaryResult returnResult;

      try
      {
        var result = RequestExecutorContainerFactory
          .Build<SummaryVolumesExecutorV2>(LoggerFactory, RaptorClient)
          .Process(request) as SummaryVolumesResult;

        returnResult = CompactionVolumesSummaryResult.Create(result, await GetProjectSettingsTargets(projectUid));
      }
      catch (ServiceException exception)
      {
        returnResult = new CompactionVolumesSummaryResult(exception.GetResult.Code, exception.GetResult.Message);
      }
      finally
      {
        Log.LogInformation("GetSummaryVolumes returned: " + Response.StatusCode);
      }

      Log.LogTrace("GetSummaryVolumes result: " + JsonConvert.SerializeObject(returnResult));

      return returnResult;
    }
  }
}