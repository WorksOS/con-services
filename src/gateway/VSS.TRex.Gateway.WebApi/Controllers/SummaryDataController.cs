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
  /// Controller for getting production data for summary requests.
  /// </summary>
  public class SummaryDataController : BaseController
  {
    private const short DATA_VALUE_NOT_REQUIRED = 0;
    private const double TEMPERATURE_TARGET_MIN = 10.5;
    private const double TEMPERATURE_TARGET_MAX = 150.5;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public SummaryDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get CMV summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="cmvSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/summary")]
    [HttpPost]
    public CompactionCmvSummaryResult PostCmvSummary([FromBody] CMVSummaryRequest cmvSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostCmvSummary)}: {Request.QueryString}");

      cmvSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvSummaryRequest) as CMVSummaryResult);

      var cmvSettings = new CMVSettings(
        cmvSummaryRequest.cmvTarget,
        DATA_VALUE_NOT_REQUIRED,
        cmvSummaryRequest.maxCMVPercent,
        DATA_VALUE_NOT_REQUIRED,
        cmvSummaryRequest.minCMVPercent,
        cmvSummaryRequest.overrideTargetCMV
      );

      return new CompactionCmvSummaryResult(result, cmvSettings);
    }

    /// <summary>
    /// Get MDP summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/mdp/summary")]
    [HttpPost]
    public CompactionMdpSummaryResult PostMdpSummary([FromBody] MDPSummaryRequest mdpSummaryRequest)
    {
      Log.LogInformation("PostMdpSummary: " + Request.QueryString);

      mdpSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryMDPExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(mdpSummaryRequest) as MDPSummaryResult);

      var mdpSettings = new MDPSettings(
        mdpSummaryRequest.mdpTarget,
        DATA_VALUE_NOT_REQUIRED,
        mdpSummaryRequest.maxMDPPercent,
        DATA_VALUE_NOT_REQUIRED,
        mdpSummaryRequest.minMDPPercent,
        mdpSummaryRequest.overrideTargetMDP
      );

      return new CompactionMdpSummaryResult(result, mdpSettings);
    }

    /// <summary>
    /// Get Pass Count summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/summary")]
    [HttpPost]
    public CompactionPassCountSummaryResult PostPassCountSummary([FromBody] PassCountSummaryRequest passCountSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostPassCountSummary)}: {Request.QueryString}");

      passCountSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(passCountSummaryRequest) as PassCountSummaryResult);

      return new CompactionPassCountSummaryResult(result);
    }

    /// <summary>
    /// Get Speed summary from production for the specified project and date range. Project UID must be provided.
    /// </summary>
    /// <param name="speedSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/speed/summary")]
    [HttpPost]
    public CompactionSpeedSummaryResult PostSpeedSummary([FromBody] SpeedSummaryRequest speedSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostSpeedSummary)}: {Request.QueryString}");

      speedSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummarySpeedExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(speedSummaryRequest) as SpeedSummaryResult);

      return new CompactionSpeedSummaryResult(result, speedSummaryRequest.machineSpeedTarget);
      ;
    }

    /// <summary>
    /// Get Temperature summary from production data for the specified project and date range.
    /// </summary>
    [Route("api/v1/temperature/summary")]
    [HttpPost]
    public CompactionTemperatureSummaryResult PostTemperatureSummary([FromBody] TemperatureSummaryRequest temperatureSummaryRequest,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation($"{nameof(PostTemperatureSummary)}: {Request.QueryString}");

      temperatureSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryTemperatureExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(temperatureSummaryRequest) as TemperatureSummaryResult);

      return new CompactionTemperatureSummaryResult(result);
    }
  }
}
