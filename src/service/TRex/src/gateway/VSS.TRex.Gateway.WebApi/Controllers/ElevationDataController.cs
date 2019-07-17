using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using ElevationDataResult = VSS.Productivity3D.Models.ResultHandling.ElevationStatisticsResult;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting elevation data from from a site model/project
  /// </summary>
  public class ElevationDataController : BaseController
  {
    public ElevationDataController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<ElevationDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get elevation range from TRex  for the specified site model/project.
    /// </summary>
    [Route("api/v1/elevationstatistics")]
    [HttpPost]
    public Task<ContractExecutionResult> PostElevationStatistics([FromBody] ElevationDataRequest elevationStatisticsRequest)
    {
      Log.LogInformation($"#In# {nameof(PostElevationStatistics)}: {Request.QueryString}");

      try
      {
        elevationStatisticsRequest.Validate();

        return WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainer
            .Build<ElevationStatisticsExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(elevationStatisticsRequest));
      }
      finally
      {
        Log.LogInformation($"#Out# {nameof(PostElevationStatistics)}: {Request.QueryString}");
      }
    }
  }
}
