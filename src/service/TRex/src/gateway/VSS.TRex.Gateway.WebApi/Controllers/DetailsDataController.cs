using System.Threading.Tasks;
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
    /// <param name="cmvChangeDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/percentchange")]
    [HttpPost]
    public async Task<CMVChangeSummaryResult> PostCmvPercentChange([FromBody] CMVChangeDetailsRequest cmvChangeDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostCmvPercentChange)}: {Request.QueryString}");

      cmvChangeDetailsRequest.Validate();
      ValidateFilterMachines(nameof(PostCmvPercentChange), cmvChangeDetailsRequest.ProjectUid, cmvChangeDetailsRequest.Filter);

      return await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DetailedCMVChangeExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(cmvChangeDetailsRequest)) as CMVChangeSummaryResult;
    }

    /// <summary>
    /// Get CMV details from production data for the specified project and date range.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/details")]
    [HttpPost]
    public async Task<CMVDetailedResult> PostCmvDetails([FromBody] CMVDetailsRequest cmvDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostCmvDetails)}: {Request.QueryString}");

      cmvDetailsRequest.Validate();
      ValidateFilterMachines(nameof(PostCmvDetails), cmvDetailsRequest.ProjectUid, cmvDetailsRequest.Filter);

      return await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DetailedCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(cmvDetailsRequest)) as CMVDetailedResult;
    }

    /// <summary>
    /// Get Pass Count details from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/details")]
    [HttpPost]
    public async Task<PassCountDetailedResult> PostPassCountDetails([FromBody] PassCountDetailsRequest passCountDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostPassCountDetails)}: {Request.QueryString}");

      passCountDetailsRequest.Validate();
      ValidateFilterMachines(nameof(PostPassCountDetails), passCountDetailsRequest.ProjectUid, passCountDetailsRequest.Filter);

      return await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DetailedPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(passCountDetailsRequest)) as PassCountDetailedResult;
    }

    /// <summary>
    /// Get cut-fill details from production data for the specified project and other parameters.
    /// </summary>
    /// <param name="cutFillRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cutfill/details")]
    [HttpPost]
    public async Task<CompactionCutFillDetailedResult> PostCutFillDetails([FromBody] CutFillDetailsRequest cutFillRequest)
    {
      Log.LogInformation($"{nameof(PostCutFillDetails)}: {Request.QueryString}");

      cutFillRequest.Validate();
      ValidateFilterMachines(nameof(PostCutFillDetails), cutFillRequest.ProjectUid, cutFillRequest.Filter);

      return await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<CutFillExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(cutFillRequest)) as CompactionCutFillDetailedResult;
    }


    /// <summary>
    /// Get Temperature details from production data for the specified project and date range.
    /// </summary>
    /// <param name="temperatureDetailRequest"></param>
    /// <returns></returns>
    [Route("api/v1/temperature/details")]
    [HttpPost]
    public async Task<TemperatureDetailResult> PostTemperatureDetails([FromBody] TemperatureDetailRequest temperatureDetailRequest)
    {
      Log.LogInformation($"{nameof(PostTemperatureDetails)}: {Request.QueryString}");
      
      temperatureDetailRequest.Validate();
      ValidateFilterMachines(nameof(PostTemperatureDetails), temperatureDetailRequest.ProjectUid, temperatureDetailRequest.Filter);

      return await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DetailedTemperatureExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(temperatureDetailRequest)) as TemperatureDetailResult;
    }
  }
}
