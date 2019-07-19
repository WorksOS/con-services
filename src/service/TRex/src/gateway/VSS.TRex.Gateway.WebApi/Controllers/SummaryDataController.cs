using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

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
      : base(loggerFactory, loggerFactory.CreateLogger<SummaryDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get CMV summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="cmvSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostCmvSummary([FromBody] CMVSummaryRequest cmvSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostCmvSummary)}: {Request.QueryString}");

      cmvSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostCmvSummary), cmvSummaryRequest.ProjectUid, cmvSummaryRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(cmvSummaryRequest));
    }


    /// <summary>
    /// Get MDP summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/mdp/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostMdpSummary([FromBody] MDPSummaryRequest mdpSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostMdpSummary)}: " + Request.QueryString);

      mdpSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostMdpSummary), mdpSummaryRequest.ProjectUid, mdpSummaryRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryMDPExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(mdpSummaryRequest));
    }

    /// <summary>
    /// Get Pass Count summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostPassCountSummary([FromBody] PassCountSummaryRequest passCountSummaryRequest)
    {
      Log.LogInformation($"#In# {nameof(PostPassCountSummary)}: {Request.QueryString}");

      try
      {
        passCountSummaryRequest.Validate();
        ValidateFilterMachines(nameof(PostPassCountSummary), passCountSummaryRequest.ProjectUid,
          passCountSummaryRequest.Filter);

        return WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainer
            .Build<SummaryPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(passCountSummaryRequest));
      }
      finally
      {
        Log.LogInformation($"#Out# {nameof(PostPassCountSummary)}: {Request.QueryString}");
      }
    }

    /// <summary>
    /// Get Speed summary from production for the specified project and date range. Project UID must be provided.
    /// </summary>
    /// <param name="speedSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/speed/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostSpeedSummary([FromBody] SpeedSummaryRequest speedSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostSpeedSummary)}: {Request.QueryString}");

      speedSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostSpeedSummary), speedSummaryRequest.ProjectUid, speedSummaryRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummarySpeedExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(speedSummaryRequest));
    }

    /// <summary>
    /// Get Temperature summary from production data for the specified project and date range.
    /// </summary>
    [Route("api/v1/temperature/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostTemperatureSummary([FromBody] TemperatureSummaryRequest temperatureSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostTemperatureSummary)}: {Request.QueryString}");

      temperatureSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostTemperatureSummary), temperatureSummaryRequest.ProjectUid, temperatureSummaryRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryTemperatureExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(temperatureSummaryRequest));
    }

    /// <summary>
    /// Get CCA summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="ccaSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cca/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostCcaSummary([FromBody] CCASummaryRequest ccaSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostCcaSummary)}: {Request.QueryString}");

      ccaSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostCcaSummary), ccaSummaryRequest.ProjectUid, ccaSummaryRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryCCAExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(ccaSummaryRequest));
    }
  }
}
