using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
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
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public VolumesDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<VolumesDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get the summary volumes report for two surfaces, producing either ground to ground, ground to design or design to ground results.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <returns></returns>
    [Route("api/v1/volumes/summary")]
    [HttpPost]
    public Task<ContractExecutionResult> PostSummaryVolumes([FromBody] SummaryVolumesDataRequest summaryVolumesRequest)
    {
      Log.LogInformation($"{nameof(PostSummaryVolumes)}: {Request.QueryString}");

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
    /// <param name="summaryVolumesProfileRequest"></param>
    /// <returns></returns>
    [Route("api/v1/volumes/summary/profile")]
    [HttpPost]
    public Task<ContractExecutionResult> PostSummaryVolumesProfile([FromBody] SummaryVolumesProfileDataRequest summaryVolumesProfileRequest)
    { 
      Log.LogInformation($"{nameof(PostSummaryVolumesProfile)}: {Request.QueryString}");
      
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
  }
}
