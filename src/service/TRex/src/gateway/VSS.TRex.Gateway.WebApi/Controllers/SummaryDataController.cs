using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
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
    public CMVSummaryResult PostCmvSummary([FromBody] CMVSummaryRequest cmvSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostCmvSummary)}: {Request.QueryString}");

      cmvSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostCmvSummary), cmvSummaryRequest.ProjectUid, cmvSummaryRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvSummaryRequest) as CMVSummaryResult);
    }


    /// <summary>
    /// Get MDP summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/mdp/summary")]
    [HttpPost]
    public MDPSummaryResult PostMdpSummary([FromBody] MDPSummaryRequest mdpSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostMdpSummary)}: " + Request.QueryString);

      mdpSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostMdpSummary), mdpSummaryRequest.ProjectUid, mdpSummaryRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryMDPExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(mdpSummaryRequest) as MDPSummaryResult);
    }

    /// <summary>
    /// Get Pass Count summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/summary")]
    [HttpPost]
    public PassCountSummaryResult PostPassCountSummary([FromBody] PassCountSummaryRequest passCountSummaryRequest)
    {
      Log.LogInformation($"#In# {nameof(PostPassCountSummary)}: {Request.QueryString}");

      try
      {
        passCountSummaryRequest.Validate();
        ValidateFilterMachines(nameof(PostPassCountSummary), passCountSummaryRequest.ProjectUid,
          passCountSummaryRequest.Filter);

        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<SummaryPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(passCountSummaryRequest) as PassCountSummaryResult);
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
    public SpeedSummaryResult PostSpeedSummary([FromBody] SpeedSummaryRequest speedSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostSpeedSummary)}: {Request.QueryString}");

      speedSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostSpeedSummary), speedSummaryRequest.ProjectUid, speedSummaryRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummarySpeedExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(speedSummaryRequest) as SpeedSummaryResult);
    }

    /// <summary>
    /// Get Temperature summary from production data for the specified project and date range.
    /// </summary>
    [Route("api/v1/temperature/summary")]
    [HttpPost]
    public TemperatureSummaryResult PostTemperatureSummary([FromBody] TemperatureSummaryRequest temperatureSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostTemperatureSummary)}: {Request.QueryString}");

      temperatureSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostTemperatureSummary), temperatureSummaryRequest.ProjectUid, temperatureSummaryRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryTemperatureExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(temperatureSummaryRequest) as TemperatureSummaryResult);
    }

    /// <summary>
    /// Get CCA summary from production data for the specified project and date range.
    /// </summary>
    /// <param name="ccaSummaryRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cca/summary")]
    [HttpPost]
    public CCASummaryResult PostCcaSummary([FromBody] CCASummaryRequest ccaSummaryRequest)
    {
      Log.LogInformation($"{nameof(PostCcaSummary)}: {Request.QueryString}");

      ccaSummaryRequest.Validate();
      ValidateFilterMachines(nameof(PostCcaSummary), ccaSummaryRequest.ProjectUid, ccaSummaryRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<SummaryCCAExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(ccaSummaryRequest) as CCASummaryResult);
    }
   
  }
}
