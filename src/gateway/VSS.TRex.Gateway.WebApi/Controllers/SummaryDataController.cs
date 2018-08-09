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

      //var cmvSummaryRequest = CMVSummaryRequest.CreateCMVSummaryRequest(projectUid, null/* filter */, 50, true, 120, 80);
      cmvSummaryRequest.Validate();

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, null)
          .Process(cmvSummaryRequest) as CMVSummaryResult);

      var cmvSettings = CMVSettings.CreateCMVSettings(
        cmvSummaryRequest.cmvTarget,
        CMV_VALUE_NOT_REQUIRED, 
        cmvSummaryRequest.maxCMVPercent,
        CMV_VALUE_NOT_REQUIRED, 
        cmvSummaryRequest.minCMVPercent, 
        cmvSummaryRequest.overrideTargetCMV
      );

      return CompactionCmvSummaryResult.Create(result, cmvSettings);
    }

    private const short CMV_VALUE_NOT_REQUIRED = 0;
  }
}
