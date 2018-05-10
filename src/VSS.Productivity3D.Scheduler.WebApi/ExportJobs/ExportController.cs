using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Hangfire.Storage;
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
      return new ScheduleJobResult { JobId = jobId };
    }

    /// <summary>
    /// Get the status of an export. When status is succeeded then also returns a file download link.
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <returns>The AWS S3 key where the file has been saved and the current state of the job</returns>
    [Route("api/v1/export/{jobId}")]
    [HttpGet]
    public JobStatusResult GetExportJobStatus(string jobId)
    {
      log.LogInformation($"GetExportJobStatus: jobId={jobId}");

      var jobData = JobStorage.Current.GetConnection()?.GetJobData(jobId);
      var status = jobData?.State;
      if (string.IsNullOrEmpty(status))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Missing job details for {jobId}"));
      }

      log.LogInformation($"GetExportJobStatus: {jobId} status={status}");
      string key = null;
      string downloadLink = null;
      FailureDetails details = null;
      if (status.Equals("SUCCEEDED", StringComparison.OrdinalIgnoreCase))
      {
        var filename = (jobData?.Job.Args[0] as ScheduleJobRequest).Filename ?? jobId;
        key = ExportJob.GetS3Key(jobId, filename);
        downloadLink = exportJob.GetDownloadLink(jobId, filename);
      }
      else if (status.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
      {
        var detailsJson = JobStorage.Current.GetConnection()?.GetStateData(jobId)?.Data[ExportFailedState.EXPORT_DETAILS_KEY];
        if (!string.IsNullOrEmpty(detailsJson))
        {
          details = JsonConvert.DeserializeObject<FailureDetails>(detailsJson);
        }
      }
      var result = new JobStatusResult { Key = key, Status = status, DownloadLink = downloadLink, FailureDetails = details };
      log.LogInformation($"GetExportJobStatus: result={JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}
