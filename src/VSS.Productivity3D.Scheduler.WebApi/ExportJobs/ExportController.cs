using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Handles requests for scheduling a long running export and getting its progress.
  /// </summary>
  public class ExportController : Controller
  {
    private IExportJob exportJob;
    private readonly ILogger log;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportController(ILoggerFactory loggerFactory, IExportJob exportJob)
    {
      log = loggerFactory.CreateLogger<ExportController>();
      this.exportJob = exportJob;
    }

    /// <summary>
    /// Schedule an export
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <returns></returns>
    [Route("internal/v1/export")]//hide from TPaaS using different base url in route ('internal' instead of 'api')
    [HttpPost]
    public ScheduleJobResult StartExport([FromBody] ScheduleJobRequest request)
    {
      log.LogInformation($"StartExport: {JsonConvert.SerializeObject(request)}");
      var jobId = BackgroundJob.Enqueue(() => exportJob.GetExportData(
        request, Request.Headers.GetCustomHeaders(true), null));
      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { jobId = jobId };
    }

    /// <summary>
    /// Get the status of an export
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <param name="filename">The name of the file the export data is saved to</param>
    /// <returns>The AWS S3 key where the file has been saved and the current state of the job</returns>
    [Route("api/v1/export/{jobId}")]
    [HttpGet]
    public JobStatusResult GetExportJobStatus(string jobId, [FromUri] string filename)
    {
      log.LogInformation($"GetExportJobStatus: jobId={jobId}, filename={filename}");

      if (string.IsNullOrEmpty(filename))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Missing file name for {jobId}"));
      }

      var status = JobStorage.Current.GetConnection()?.GetJobData(jobId)?.State;
      if (string.IsNullOrEmpty(status))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Missing job details for {jobId}"));
      }

      log.LogInformation($"GetExportJobStatus: {jobId} status={status}");
      string downloadLink = null;
      if (status.Equals("SUCCEEDED", StringComparison.OrdinalIgnoreCase))
      {
        downloadLink = exportJob.GetDownloadLink(jobId, filename);
        log.LogInformation($"GetExportJobStatus: {jobId} downloadLink={downloadLink}");
      }

      return new JobStatusResult { key = ExportJob.GetS3Key(jobId, filename), status = status, downloadLink = downloadLink };
    }
  }
}
