using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
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
    private readonly ITransferProxy transferProxy;
    private readonly ILogger log;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportController(ILoggerFactory loggerFactory, IExportJob exportJob, ITransferProxy transferProxy)
    {
      log = loggerFactory.CreateLogger<ExportController>();
      this.exportJob = exportJob;
      this.transferProxy = transferProxy;
    }

    /// <summary>
    /// Schedule an export
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <returns></returns>
    [Route("internal/v1/background")] // double up the url with the intention of splitting this later
    [Route("internal/v1/export")]//hide from TPaaS using different base url in route ('internal' instead of 'api')
    [HttpPost]
    public ScheduleJobResult StartExport([FromBody] ScheduleJobRequest request)
    {
      log.LogInformation($"StartExport: Url {request?.Url}");
      string jobId;
      try
      {
       jobId = BackgroundJob.Enqueue(() => exportJob.GetExportData(
        request, Request.Headers.GetCustomHeaders(true), null));
      }
      catch (Exception e)
      {
        Console.WriteLine($"SchedulerWebAPI exception {JsonConvert.SerializeObject(e)} ");
        throw;
      }

      Console.WriteLine($"SchedulerWebAPI after StartExport jobId: {jobId}");
      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = jobId };
    }

    /// <summary>
    /// Get the status of an export. When status is succeeded then also returns a file download link.
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <returns>The AWS S3 key where the file has been saved and the current state of the job</returns>
    [Route("api/v1/background/{jobId}")]  // double up the url with the intention of splitting this later
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
      if (status.Equals(Hangfire.States.SucceededState.StateName, StringComparison.OrdinalIgnoreCase))
      {
        // Attempt to get the download link that should ve set in the job
        key = JobStorage.Current.GetConnection().GetJobParameter(jobId, ExportJob.S3KeyStateKey);
        downloadLink = JobStorage.Current.GetConnection().GetJobParameter(jobId, ExportJob.DownloadLinkStateKey);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(downloadLink))
        {
          log.LogWarning("S3Key or Downloadlink not set in background job, attempting to find it via the original request");
          var filename = (jobData?.Job.Args[0] as ScheduleJobRequest).Filename ?? jobId;
          key = ExportJob.GetS3Key(jobId, filename);
          downloadLink = exportJob.GetDownloadLink(jobId, filename);
        }
      }
      else if (status.Equals(Hangfire.States.FailedState.StateName, StringComparison.OrdinalIgnoreCase))
      {
        var detailsJson = JobStorage.Current.GetConnection()?.GetStateData(jobId)?.Data[ExportFailedState.EXPORT_DETAILS_KEY];
        log.LogDebug($"GetExportJobStatus: detailsJson={detailsJson}");
        if (!string.IsNullOrEmpty(detailsJson))
        {
          details = JsonConvert.DeserializeObject<FailureDetails>(detailsJson);
        }
      }
      var result = new JobStatusResult { Key = key, Status = status, DownloadLink = downloadLink, FailureDetails = details };
      log.LogInformation($"GetExportJobStatus: result={JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Get the result for a background job
    /// </summary>
    /// <param name="jobId">Job Id of the background job</param>
    /// <returns>The content of the results of the background job if the job is completed</returns>
    [Route("api/v1/export/{jobId}/result")]
    [Route("api/v1/background/{jobId}/result")]
    public FileStreamResult GetExportJobResult(string jobId)
    {
      var status = GetExportJobStatus(jobId);

      if (string.IsNullOrEmpty(status.Key))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Missing job download link for {jobId}"));
      }

      return transferProxy.Download(status.Key).Result;
    }
  }
}
