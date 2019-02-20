using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using FileSystem = System.IO.File;


namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// The controller for generating Csv file of productionData
  /// </summary>
  public class CSVExportController : BaseController
  {
    /// <summary>
    /// Constructor for csv export controller
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public CSVExportController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<CSVExportController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Web service end point controller for veta export of csv file
    /// </summary>
    /// <param name="compactionVetaExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/veta")]
    public CompactionExportResult GetVetaExport([FromBody] CompactionVetaExportRequest compactionVetaExportRequest)
    {
      Log.LogInformation($"{nameof(GetVetaExport)}: {Request.QueryString}");

      compactionVetaExportRequest.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(compactionVetaExportRequest);

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CSVExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(compactionCSVExportRequest) as CompactionExportResult);

      return result;
    }

    /// <summary>
    /// Web service end point controller for PassCount export of csv file
    /// </summary>
    /// <param name="compactionPassCountExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/passcount")]
    public CompactionExportResult GetPassCountExport([FromBody] CompactionPassCountExportRequest compactionPassCountExportRequest)
    {
      Log.LogInformation($"{nameof(GetPassCountExport)}: {Request.QueryString}");

      compactionPassCountExportRequest.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(compactionPassCountExportRequest);

      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CSVExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(compactionCSVExportRequest) as CompactionExportResult);

      return result;
    }
  }
}
