using System;
using System.Net;
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
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
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
  [ProjectVerifier]
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionDetailsDataController : CompactionDataBaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionDetailsDataController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory)
      : base(raptorClient, configStore, fileListProxy, settingsManager, requestFactory)
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
      var projectId = await GetLegacyProjectId(projectUid);

      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(
        projectId, null, liftSettings, filter, -1, cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainerFactory.Build<CMVChangeSummaryExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVChangeSummaryResult;
        var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result);
        Log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));

        await SetCacheControlPolicy(projectUid);

        return returnResult;
      }
      catch (ServiceException exception)
      {
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));

        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. 
    /// </summary>
    [Route("api/v2/cmv/details/targets")]
    [HttpGet]
    [Obsolete("Use 'cmv/details' for v2 and 'compaction/cmv/detailed' for v1 result")]
    public async Task<CompactionCmvDetailedResult> GetCmvDetailsTargets(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvDetailsTargets: " + Request.QueryString);

      CMVRequest request = await GetCmvRequest(projectUid, filterUid);
      request.Validate();

      try
      {
        var result = RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVDetailedResult;

        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result, null, null);
        Log.LogInformation("GetCmvDetailsTargets result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));

        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
      }
      finally
      {
        Log.LogInformation("GetCmvDetailsTargets returned: " + Response.StatusCode);
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

      CMVRequest request = await GetCmvRequest(projectUid, filterUid, true);
      request.Validate();

      try
      {
        var result1 = RequestExecutorContainerFactory.Build<DetailedCMVExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVDetailedResult;

        var result2 = RequestExecutorContainerFactory
          .Build<SummaryCMVExecutor>(LoggerFactory, RaptorClient)
          .Process(request) as CMVSummaryResult;

        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result1, result2, request.cmvSettings);

        Log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException exception)
      {
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));

        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
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
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, exception.Message));

        var statusCode = (TASNodeErrorStatus)exception.GetResult.Code == TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;

        throw new ServiceException(statusCode,
          new ContractExecutionResult(exception.GetResult.Code, exception.GetResult.Message));
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
      var projectId = await GetLegacyProjectId(projectUid);

      var cutFillRequest = RequestFactory.Create<CutFillRequestHelper>(r => r
          .ProjectId(projectId)
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
