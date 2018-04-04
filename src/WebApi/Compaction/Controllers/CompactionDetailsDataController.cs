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
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
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
  /// Controller for getting Raptor production data for details requests.
  /// </summary>
  [ProjectUidVerifier]
  // [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })] (Aaron) Disabled temporarily until we can resolve the cache invalidation problem
  [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
  public class CompactionDetailsDataController : CompactionDataBaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionDetailsDataController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IConfigurationStore configStore, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy, IProductionDataRequestFactory requestFactory)
      : base(raptorClient, loggerFactory, loggerFactory.CreateLogger<CompactionDetailsDataController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager, requestFactory)
    { }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cmv/percentchange")]
    [HttpGet]
    public async Task<CompactionCmvPercentChangeResult> GetCmvPercentChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvPercentChange: " + Request.QueryString);

      if (!await ValidateFilterAgainstProjectExtents(projectUid, filterUid))
      {
        return CompactionCmvPercentChangeResult.CreateEmptyResult();
      }

      var projectSettings = await this.GetProjectSettingsTargets(projectUid);
      LiftBuildSettings liftSettings = this.SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      double[] cmvChangeSummarySettings = this.SettingsManager.CompactionCmvPercentChangeSettings(projectSettings);
      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(
        GetLegacyProjectId(projectUid), null, liftSettings, filter, -1, cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVChangeSummaryResult;
        var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result);
        Log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));

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
        Log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/cmv/details")]
    [HttpGet]
    public async Task<CompactionCmvDetailedResult> GetCmvDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvDetails: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVDetailedResult;
        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result);

        Log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count details from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v2/passcounts/details")]
    [HttpGet]
    public async Task<CompactionPassCountDetailedResult> GetPassCountDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetPassCountDetails: " + Request.QueryString);

      PassCounts request = await GetPassCountRequest(projectUid, filterUid, false);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory
                     .Build<DetailedPassCountExecutor>(LoggerFactory, RaptorClient)
                     .Process(request) as PassCountDetailedResult;

        var returnResult = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(result);

        Log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));
      }
      finally
      {
        Log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
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
      Log.LogInformation("GetCutFillDetails: " + Request.QueryString);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      var cutFillRequest = RequestFactory.Create<CutFillRequestHelper>(r => r
          .ProjectId(GetLegacyProjectId(projectUid))
          .Headers(this.CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter)
          .DesignDescriptor(cutFillDesign))
        .CreateCutFillDetailsRequest();

      cutFillRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionCutFillExecutor>(LoggerFactory, RaptorClient)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }
  }
}