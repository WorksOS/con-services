using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data for volumes statistics requests.
  /// </summary>
  public class VolumesDataController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public VolumesDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<VolumesDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    [Route("api/v1/volumes/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostSummaryVolumes([FromBody] SummaryVolumesDataRequest summaryVolumesRequest)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {JsonConvert.SerializeObject(summaryVolumesRequest)}");

      summaryVolumesRequest.Validate();
      if (summaryVolumesRequest.ProjectUid == null || summaryVolumesRequest.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      ValidateFilterMachines(nameof(PostSummaryVolumes), summaryVolumesRequest.ProjectUid, summaryVolumesRequest.BaseFilter);
      ValidateFilterMachines(nameof(PostSummaryVolumes), summaryVolumesRequest.ProjectUid, summaryVolumesRequest.TopFilter);
      ValidateFilterMachines(nameof(PostSummaryVolumes), summaryVolumesRequest.ProjectUid, summaryVolumesRequest.AdditionalSpatialFilter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryVolumesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(summaryVolumesRequest));
    }

    /// <summary>
    /// Get the summary volumes profile report for two surfaces.
    /// </summary>
    [Route("api/v1/volumes/summary/profile")]
    [HttpPost]
    public Task<ContractExecutionResult> PostSummaryVolumesProfile([FromBody] SummaryVolumesProfileDataRequest summaryVolumesProfileRequest)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumesProfile)}: {JsonConvert.SerializeObject(summaryVolumesProfileRequest)}");

      summaryVolumesProfileRequest.Validate();
      if (summaryVolumesProfileRequest.ProjectUid == null || summaryVolumesProfileRequest.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      ValidateFilterMachines(nameof(PostSummaryVolumesProfile), summaryVolumesProfileRequest.ProjectUid, summaryVolumesProfileRequest.Filter);
      ValidateFilterMachines(nameof(PostSummaryVolumesProfile), summaryVolumesProfileRequest.ProjectUid, summaryVolumesProfileRequest.TopFilter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<SummaryVolumesProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(summaryVolumesProfileRequest));
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results,
    /// over the course of a progressive series defined by start and end times, and an interval.
    /// </summary>
    [Route("api/v1/volumes/summary/progressive")]
    [HttpPost]
    public Task<ContractExecutionResult> PostProgressiveSummaryVolumes([FromBody] ProgressiveSummaryVolumesDataRequest request)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      if (request.ProjectUid == null || request.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      ValidateFilterMachines(nameof(PostProgressiveSummaryVolumes), request.ProjectUid, request.Filter);
      ValidateFilterMachines(nameof(PostProgressiveSummaryVolumes), request.ProjectUid, request.AdditionalSpatialFilter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<ProgressiveSummaryVolumesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(request));
    }
  }
}
