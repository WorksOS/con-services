using System;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary data requests
  /// </summary>
  /// <remarks>
  /// There is no response caching for this provided because at the moment the caching middleware doesn't handle requests with more than one filter.
  /// </remarks>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CompactionSummaryDataController : CompactionDataBaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionSummaryDataController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, ITRexCompactionDataProxy trexCompactionDataProxy)
      : base(raptorClient, configStore, fileListProxy, settingsManager, requestFactory, trexCompactionDataProxy)
    { }

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/cmv/summary")]
    [HttpGet]
    public async Task<ActionResult<CompactionCmvSummaryResult>> GetCmvSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvSummary: " + Request.QueryString);

      var request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      var (isValidFilterForProjectExtents, _) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return Ok(new CompactionCmvSummaryResult());
      }

      Log.LogDebug("GetCmvSummary request for Raptor: " + JsonConvert.SerializeObject(request));
      try
      {
        var result = RequestExecutorContainerFactory
              .Build<SummaryCMVExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
              .Process(request) as CMVSummaryResult;

        var cmvSummaryResult = new CompactionCmvSummaryResult(result, request.CmvSettings);
        Log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(cmvSummaryResult));

        await SetCacheControlPolicy(projectUid);

        return Ok(cmvSummaryResult);
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetCmvSummary)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
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
    [ProjectVerifier]
    [Route("api/v2/mdp/summary")]
    [HttpGet]
    public async Task<ActionResult<CompactionMdpSummaryResult>> GetMdpSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetMdpSummary: " + Request.QueryString);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var mdpSettings = SettingsManager.CompactionMdpSettings(projectSettings);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var (isValidFilterForProjectExtents, filter) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return Ok(new CompactionMdpSummaryResult());
      }

      if (filter == null) await GetCompactionFilter(projectUid, filterUid);

      var projectId = await GetLegacyProjectId(projectUid);
      var request = new MDPRequest(projectId, projectUid, null, mdpSettings, liftSettings, filter, -1, null, null, null);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<SummaryMDPExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(request) as MDPSummaryResult;

        var mdpSummaryResult = new CompactionMdpSummaryResult(result, mdpSettings);
        Log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(mdpSummaryResult));

        await SetCacheControlPolicy(projectUid);

        return Ok(mdpSummaryResult);
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetMdpSummary)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/passcounts/summary")]
    [HttpGet]
    public async Task<ActionResult<CompactionPassCountSummaryResult>> GetPassCountSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      var (isValidFilterForProjectExtents, filter) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return Ok(new CompactionPassCountSummaryResult());
      }
      
      var request = await GetPassCountRequest(projectUid, filterUid, filter, isSummary: true);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<SummaryPassCountsExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(request) as PassCountSummaryResult;

        var passCountSummaryResult = new CompactionPassCountSummaryResult(result);
        Log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(passCountSummaryResult));

        await SetCacheControlPolicy(projectUid);

        return Ok(passCountSummaryResult);
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetPassCountSummary)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/temperature/summary")]
    [HttpGet]
    public async Task<ActionResult<CompactionTemperatureSummaryResult>> GetTemperatureSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetTemperatureSummary: " + Request.QueryString);

      var (isValidFilterForProjectExtents, filter) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return Ok(new CompactionTemperatureSummaryResult());
      }

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var temperatureSettings = SettingsManager.CompactionTemperatureSettings(projectSettings, false);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      if (filter == null) await GetCompactionFilter(projectUid, filterUid);

      var projectId = await GetLegacyProjectId(projectUid);
      var request = new TemperatureRequest(projectId, projectUid, null, temperatureSettings, liftSettings, filter, -1, null, null, null);

      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<SummaryTemperatureExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(request) as TemperatureSummaryResult;

        var temperatureSummaryResult = new CompactionTemperatureSummaryResult(result);
        Log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(temperatureSummaryResult));

        await SetCacheControlPolicy(projectUid);

        return Ok(temperatureSummaryResult);
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetTemperatureSummary)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/speed/summary")]
    [HttpGet]
    public async Task<ActionResult<CompactionSpeedSummaryResult>> GetSpeedSummary(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetSpeedSummary: " + Request.QueryString);

      var projectId = await GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var (isValidFilterForProjectExtents, filter) = await ValidateFilterAgainstProjectExtents(projectUid, filterUid);
      if (!isValidFilterForProjectExtents)
      {
        return Ok(new CompactionSpeedSummaryResult());
      }

      if (filter == null) await GetCompactionFilter(projectUid, filterUid);

      SummarySpeedRequest request = new SummarySpeedRequest(projectId, projectUid, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<SummarySpeedExecutor>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(request) as SpeedSummaryResult;

        var speedSummaryResult = new CompactionSpeedSummaryResult(result, liftSettings.MachineSpeedTarget);
        Log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(speedSummaryResult));

        await SetCacheControlPolicy(projectUid);

        return Ok(speedSummaryResult);
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetSpeedSummary)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
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
    [ProjectVerifier]
    [Route("api/v2/volumes/summary")]
    [HttpGet]
    public async Task<ActionResult<ContractExecutionResult>> GetSummaryVolumes(
      [FromServices] ISummaryDataHelper summaryDataHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid baseUid,
      [FromQuery] Guid topUid)
    {
      Log.LogInformation("GetSummaryVolumes: " + Request.QueryString);

      if (baseUid == Guid.Empty || topUid == Guid.Empty)
      {
        return BadRequest(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid surface parameter(s)."));
      }

      DesignDescriptor baseDesign = null;
      DesignDescriptor topDesign = null;
      FilterResult baseFilter = null;
      FilterResult topFilter = null;

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
        return BadRequest(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Can not resolve either baseSurface or topSurface"));
      }

      var volumeCalcType = summaryDataHelper.GetVolumesType(baseFilter, topFilter);

      if (volumeCalcType == VolumesType.None)
      {
        return BadRequest(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing volumes calculation type"));
      }
      var projectId = await GetLegacyProjectId(projectUid);
      var request = SummaryVolumesRequest.CreateAndValidate(projectId, projectUid, baseFilter, topFilter, baseDesign, topDesign, volumeCalcType);

      CompactionVolumesSummaryResult volumesSummaryResult;

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<SummaryVolumesExecutorV2>(LoggerFactory, RaptorClient, configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
                     .Process(request) as SummaryVolumesResult;

        volumesSummaryResult = CompactionVolumesSummaryResult.Create(result, await GetProjectSettingsTargets(projectUid));
      }
      catch (ServiceException exception) when ((TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics)
      {
        return NoContent();
      }
      catch (ServiceException exception)
      {
        Log.LogError($"{nameof(GetSummaryVolumes)}: {exception.GetResult.Message} ({exception.GetResult.Code})");
        return BadRequest(new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetSummaryVolumes returned: " + Response.StatusCode);
      }

      Log.LogTrace("GetSummaryVolumes result: " + JsonConvert.SerializeObject(volumesSummaryResult));

      return Ok(volumesSummaryResult);
    }
  }
}
