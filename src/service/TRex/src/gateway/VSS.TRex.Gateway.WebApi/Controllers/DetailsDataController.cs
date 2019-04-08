using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;

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
    public CMVChangeSummaryResult PostCmvPercentChange([FromBody] CMVChangeDetailsRequest cmvChangeDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostCmvPercentChange)}: {Request.QueryString}");

      cmvChangeDetailsRequest.Validate();
      ValidateFilterMachines(cmvChangeDetailsRequest.ProjectUid, cmvChangeDetailsRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedCMVChangeExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvChangeDetailsRequest) as CMVChangeSummaryResult);
    }

    /// <summary>
    /// Get CMV details from production data for the specified project and date range.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/cmv/details")]
    [HttpPost]
    public CMVDetailedResult PostCmvDetails([FromBody] CMVDetailsRequest cmvDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostCmvDetails)}: {Request.QueryString}");

      cmvDetailsRequest.Validate();
      ValidateFilterMachines(cmvDetailsRequest.ProjectUid, cmvDetailsRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedCMVExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cmvDetailsRequest) as CMVDetailedResult);
    }

    /// <summary>
    /// Get Pass Count details from production data for the specified project and date range.
    /// </summary>
    /// <param name="passCountDetailsRequest"></param>
    /// <returns></returns>
    [Route("api/v1/passcounts/details")]
    [HttpPost]
    public PassCountDetailedResult PostPassCountDetails([FromBody] PassCountDetailsRequest passCountDetailsRequest)
    {
      Log.LogInformation($"{nameof(PostPassCountDetails)}: {Request.QueryString}");

      passCountDetailsRequest.Validate();
      ValidateFilterMachines(passCountDetailsRequest.ProjectUid, passCountDetailsRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedPassCountExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(passCountDetailsRequest) as PassCountDetailedResult);
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
      ValidateFilterMachines(cutFillRequest.ProjectUid, cutFillRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CutFillExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(cutFillRequest) as CompactionCutFillDetailedResult);
    }


    /// <summary>
    /// Get Temperature details from production data for the specified project and date range.
    /// </summary>
    /// <param name="temperatureDetailRequest"></param>
    /// <returns></returns>
    [Route("api/v1/temperature/details")]
    [HttpPost]
    public TemperatureDetailResult PostTemperatureDetails([FromBody] TemperatureDetailRequest temperatureDetailRequest)
    {
      Log.LogInformation($"{nameof(PostTemperatureDetails)}: {Request.QueryString}");
      
      temperatureDetailRequest.Validate();
      ValidateFilterMachines(temperatureDetailRequest.ProjectUid, temperatureDetailRequest.Filter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DetailedTemperatureExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(temperatureDetailRequest) as TemperatureDetailResult);
    }

    private void ValidateFilterMachines(Guid? projectUid, FilterResult filterResult)
    {
      if (projectUid == null || projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(projectUid.Value, nameof(DetailsDataController));
      if (filterResult != null && filterResult.ContributingMachines != null)
        GatewayHelper.ValidateMachines(filterResult.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);
    }
  }
}
