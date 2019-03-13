using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;


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
    /// 
    /// TRex stores the exported file on s3 at: AWS_BUCKET_NAME e.g. vss-exports-stg/prod
    ///           this bucket is more temporary than other buckets (designs and tagFiles)
    ///
    /// the response fullFileName is in format: "project/{projectUId}/TRexExport/{request.FileName}__{uniqueTRexUid}.zip",
    ///                                    e.g. "project/f13f2458-6666-424f-a995-4426a00771ae/TRexExport/blahDeBlahAmy__70b0f407-67a8-42f6-b0ef-1fa1d36fc71c.zip"
    /// </summary>
    /// <param name="compactionVetaExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/veta")]
    public CompactionExportResult GetVetaExport([FromBody] CompactionVetaExportRequest compactionVetaExportRequest)
    {
      Log.LogInformation($"{nameof(GetVetaExport)}: {Request.QueryString}");

      compactionVetaExportRequest.Validate();
      return Execute(compactionVetaExportRequest);
    }

    /// <summary>
    /// Web service end point controller for PassCount export of csv file
    ///    see GetVetaExport() summary re destination of exportedData file
    /// </summary>
    /// <param name="compactionPassCountExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/passcount")]
    public CompactionExportResult GetPassCountExport([FromBody] CompactionPassCountExportRequest compactionPassCountExportRequest)
    {
      Log.LogInformation($"{nameof(GetPassCountExport)}: {Request.QueryString}");

      compactionPassCountExportRequest.Validate();
      return Execute(compactionPassCountExportRequest);
    }

    private CompactionExportResult Execute<T>(T request)
    {
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<CSVExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(compactionCSVExportRequest) as CompactionExportResult);

    }
  }
}
