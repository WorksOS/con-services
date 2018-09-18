using System;
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
    /// Get CMV % change from Raptor for the specified project and date range.
    /// </summary>
    [Route("api/v1/cmv/percentchange")]
    [HttpGet]
    public CompactionCmvPercentChangeResult GetCmvPercentChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation($"{nameof(GetCmvPercentChange)}: {Request.QueryString}");

      var cmvChangeDetailsRequest = new CMVChangeDetailsRequest(projectUid, null/* filter */, new[] { -50.0, -10.0, 0.0, 10.0, 50.0 });
      cmvChangeDetailsRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedCMVChangeExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvChangeDetailsRequest) as CMVChangeSummaryResult);

      return new CompactionCmvPercentChangeResult(result);
    }

    /// <summary>
    /// Get CMV details from production data for the specified project and date range.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/details")]
    [HttpPost]
    public CompactionCmvDetailedResult PostCmvDetails([FromBody] CMVDetailsRequest cmvDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostCmvDetails)}: {Request.QueryString}");

      cmvDetailsRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvDetailsRequest) as CMVDetailedResult);

      return new CompactionCmvDetailedResult(result, null, null);
    }

    /// <summary>
    /// Get Pass Count details from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/details")]
    [HttpPost]
    public CompactionPassCountDetailedResult PostPassCountDetails([FromBody] PassCountDetailsRequest passCountDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostPassCountDetails)}: {Request.QueryString}");

      passCountDetailsRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(passCountDetailsRequest) as PassCountDetailedResult);

      return new CompactionPassCountDetailedResult(result);
    }

    /// <summary>
    /// Get cut-fill details from production data for the specified project and other parameters.
    /// </summary>
    /// <param name="cutFillRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cutfill/details")]
    [HttpPost]
    public CompactionCutFillDetailedResult PostCutFillDetails([FromBody] CutFillDetailsRequest cutFillRequest)
    {
      Log.LogInformation($"{nameof(PostCutFillDetails)}: {Request.QueryString}");

      cutFillRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CutFillExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }
  }
}
