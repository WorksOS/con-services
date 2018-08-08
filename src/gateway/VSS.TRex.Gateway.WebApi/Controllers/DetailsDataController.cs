﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data for details requests.
  /// </summary>
  public class DetailsDataController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public DetailsDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get CMV details from production data for the specified project and date range.
    /// </summary>
    [Route("api/v1/cmv/details")]
    [HttpGet]
    public CompactionCmvDetailedResult GetCmvDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetCmvDetails: " + Request.QueryString);

      // TODO...
      //var projectSettings = await GetProjectSettingsTargets(projectUid);
      //var filter = await GetCompactionFilter(projectUid, filterUid);

      var cmvDetailsRequest = CMVDetailsRequest.CreateCMVDetailsRequest(projectUid, null/* filter */, new[] { 0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700 });
      cmvDetailsRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, null)
          .Process(cmvDetailsRequest) as CMVDetailedResult);

      return CompactionCmvDetailedResult.CreateCmvDetailedResult(result);
    }

    /// <summary>
    /// Get cut-fill details from production data for the specified project and other parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filterUid"></param>
    /// <param name="cutfillDesignUid"></param>
    /// <returns></returns>
    [Route("api/v1/cutfill/details")]
    [HttpGet]
    public CompactionCutFillDetailedResult GetCutFillDetails(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid cutfillDesignUid)
    {
      Log.LogInformation("GetCutFillDetails: " + Request.QueryString);

      // TODO...
      //var projectSettings = await GetProjectSettingsTargets(projectUid);
      //var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      //var filter = await GetCompactionFilter(projectUid, filterUid);

      var cutFillDesign = DesignDescriptor.CreateDesignDescriptor(-1, null, 0.0, cutfillDesignUid);

      var cutFillRequest = CutFillDetailsRequest.CreateCutFillDetailsRequest(projectUid, new [] { 0.2, 0.1, 0.05, 0, -0.05, -0.1, -0.2 }, null/* filter */, cutFillDesign);

      cutFillRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CutFillExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, null)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }
  }
}
