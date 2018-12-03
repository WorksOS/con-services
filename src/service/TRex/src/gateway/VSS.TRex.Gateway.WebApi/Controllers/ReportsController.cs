using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting report data.
  /// </summary>
  public class ReportsController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public ReportsController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get station-offset report stream for the specified project, filter etc.
    /// </summary>
    /// <param name="stationOffsetRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/stationoffset")]
    [HttpPost]
    public Stream PostStationOffsetReport([FromBody] CompactionReportStationOffsetRequest stationOffsetRequest)
    {
      Log.LogInformation($"{nameof(PostStationOffsetReport)}: {Request.QueryString}");

      stationOffsetRequest.Validate();

      throw new ServiceException(HttpStatusCode.NotImplemented, 
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"StationOffset report has not been implemented in Trex yet. ProjectUid: {stationOffsetRequest.ProjectUid}"));
    }

    /// <summary>
    /// Get grid report for the specified project, filter etc.
    /// </summary>
    /// <param name="gridRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/grid")]
    [HttpPost]
    public Stream PostGridReport([FromBody] CompactionReportGridRequest gridRequest)
    {
      Log.LogInformation($"{nameof(CompactionReportGridRequest)}: {Request.QueryString}");

      gridRequest.Validate();

      throw new ServiceException(HttpStatusCode.NotImplemented,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Grid report has not been implemented in Trex yet. ProjectUid: {gridRequest.ProjectUid}"));
    }
  }
}
