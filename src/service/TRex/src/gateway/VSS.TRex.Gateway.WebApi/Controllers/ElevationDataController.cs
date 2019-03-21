using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
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
    public ElevationDataResult PostElevationStatistics([FromBody] ElevationDataRequest elevationStatisticsRequest)
    {
      Log.LogInformation($"{nameof(PostElevationStatistics)}: {Request.QueryString}");

      elevationStatisticsRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<ElevationStatisticsExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(elevationStatisticsRequest) as ElevationDataResult);
    }
  }
}
