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

      var cmvSettings = CMVSettings.CreateCMVSettings(
        cmvSummaryRequest.cmvTarget,
        DATA_VALUE_NOT_REQUIRED, 
        cmvSummaryRequest.maxCMVPercent,
        DATA_VALUE_NOT_REQUIRED, 
        cmvSummaryRequest.minCMVPercent, 
        cmvSummaryRequest.overrideTargetCMV
      );

      return CompactionCmvSummaryResult.Create(result, cmvSettings);
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

      var mdpSettings = MDPSettings.CreateMDPSettings(
        mdpSummaryRequest.mdpTarget,
        DATA_VALUE_NOT_REQUIRED,
        mdpSummaryRequest.maxMDPPercent,
        DATA_VALUE_NOT_REQUIRED,
        mdpSummaryRequest.minMDPPercent,
        mdpSummaryRequest.overrideTargetMDP
      );

      return CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
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

      return CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
    }
  }
}
