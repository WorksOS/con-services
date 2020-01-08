using System;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Abstractions.UINotifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.ExportJob;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Handles requests for scheduling a long running export and getting its progress.
  /// </summary>
  public class ExportController : Controller
  {
    private IExportJob exportJob;
    private readonly ITransferProxy transferProxy;
    private readonly ILogger log;
    private readonly IJobRunner jobRunner;

    // todoJeannie for manual test endpoints
    private readonly INotificationHubClient _notificationHubClient;
    private IProjectEventHubClient _projectEventHubClient;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportController(ILoggerFactory loggerFactory, IExportJob exportJob, ITransferProxy transferProxy, IJobRunner jobRunner,
      INotificationHubClient notificationHubClient,
      IProjectEventHubClient projectEventHubClient)
    {
      log = loggerFactory.CreateLogger<ExportController>();
      this.exportJob = exportJob;
      this.transferProxy = transferProxy;
      this.jobRunner = jobRunner;
      _notificationHubClient = notificationHubClient;
      _projectEventHubClient = projectEventHubClient; // todoJeannie
    }

    // todoJeannie temp for testing
    //   For this test, ensure that in Startup.ConfigureAdditionalServices
    //      ProjectEventHubClient is NOT setup as a hostedService, use: AddPushServiceClientNonHosted()
    //      This is because PushSvc requires customerUid from headers.
    //         If the projectEventHubClient connects via hostedServices,
    //           there will be no headers, so we need to control connection manually.
    //   also, env vars for projectSvc e.g. project_service_public_v4 http://localhost:5001/api/v4
    //      possibly custSvc - depending on token type (application/client)
    [Route("JeannieTestSubscribeToProjectEvents")]
    [HttpPost]
    public async Task<ContractExecutionResult> JeannieTestSubscribeToProjectEvents()
    {
      log.LogInformation($"{nameof(JeannieTestSubscribeToProjectEvents)}: starting up");

      var DIMENSIONS_PROJECT_UID = "ff91dd40-1569-4765-a2bc-014321f76ace"; // so MockService returns something
      var importedFileStatus = new ImportedFileStatus(Guid.Parse(DIMENSIONS_PROJECT_UID), Guid.NewGuid());

      IProjectEventHubClient projectEventHubClient = null;
      try
      {
        projectEventHubClient = await ProjectEventHubConnect();
        await projectEventHubClient.SubscribeToProjectEvents(importedFileStatus.ProjectUid);
        await projectEventHubClient.FileImportIsComplete(importedFileStatus);
      }
      catch (Exception e)
      {
        log.LogError(e, $"{nameof(JeannieTestSubscribeToProjectEvents)}: exception: ");
      }
      await ProjectEventHubDisConnect(projectEventHubClient);

      return new ContractExecutionResult();
    }

    // todoJeannie temp for testing
    //   For this test, ensure that in Startup.ConfigureAdditionalServices
    //      ProjectEventHubClient is setup as a hostedService i.e. AddPushServiceClient()
    [Route("JeannieTestProjectEvent")]
    [HttpPost]
    public async Task<ContractExecutionResult> JeannieTestProjectEvent()
    {
      log.LogInformation($"{nameof(JeannieTestProjectEvent)}: starting up");

      var importedFileStatus = new ImportedFileStatus(Guid.NewGuid(), Guid.NewGuid());
      await _projectEventHubClient.FileImportIsComplete(importedFileStatus);

      return new ContractExecutionResult();
    }

    // todoJeannie temp for testing
    [Route("JeannieTestNotificationEvent")]
    [HttpPost]
    public async Task<ContractExecutionResult> JeannieTestNotificationEvent()
    {
      log.LogInformation($"{nameof(JeannieTestNotificationEvent)}: starting up");

      var notifyParams = new RasterTileNotificationParameters
      {
        FileUid = Guid.NewGuid(),
        MinZoomLevel = 14,
        MaxZoomLevel = 16
      };
      await _notificationHubClient.Notify(new ProjectFileRasterTilesGeneratedNotification(notifyParams));
      return new ContractExecutionResult();
    }

    private async Task<IProjectEventHubClient> ProjectEventHubConnect()
    {
      var projectEventHubClient = HttpContext.RequestServices.GetService<IProjectEventHubClient>();
      
      var headers = Request.Headers.GetCustomHeaders();
      projectEventHubClient.SetupHeaders(headers); 

      await projectEventHubClient.ConnectAndWait();
      return projectEventHubClient;
    }

    private async Task ProjectEventHubDisConnect(IProjectEventHubClient projectEventHubClient)
    {
      if (projectEventHubClient != null && 
          (projectEventHubClient.Connected || projectEventHubClient.IsConnecting))
      await projectEventHubClient.Disconnect();
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

      JobRequest jobRequest = new JobRequest() { JobUid = Guid.Parse("c3cbb048-05c1-4961-a799-70434cb2f162"), SetupParameters = request, RunParameters = Request.Headers.GetCustomHeaders(true) };

      log.LogInformation($"{nameof(StartExport)}: {JsonConvert.SerializeObject(request)}");
      jobRequest.Validate();
      jobRequest.AttributeFilters = SpecialFilters.ExportFilter;
      string hangfireJobId;
      try
      {
        hangfireJobId = jobRunner.QueueHangfireJob(jobRequest);
      }
      catch (Exception e)
      {
        log.LogError($"Queue VSS job failed with exception {e.Message}", e);
        throw;
      }

      //Hangfire will substitute a PerformContext automatically
      return new ScheduleJobResult { JobId = hangfireJobId };
    }

    /// <summary>
    /// Get the status of an export. When status is succeeded then also returns a file download link.
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <returns>The AWS S3 key where the file has been saved and the current state of the job</returns>
    [Route("api/v1/background/{jobId}")]  // double up the url with the intention of splitting this later
    [Route("api/v1/export/{jobId}")]
    [Route("internal/v1/background/{jobId}")]  // double up the url with the intention of splitting this later
    [Route("internal/v1/export/{jobId}")]
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
      else if (status.Equals(Hangfire.States.DeletedState.StateName, StringComparison.OrdinalIgnoreCase))
      {
        var detailsJson = JobStorage.Current.GetConnection().GetJobParameter(jobId, ExportFailedState.EXPORT_DETAILS_KEY);
        log.LogDebug($"GetExportJobStatus: detailsJson={detailsJson}");
        if (!string.IsNullOrEmpty(detailsJson))
        {
          details = JsonConvert.DeserializeObject<FailureDetails>(detailsJson);
        }
      }

      // Change behavior so it's not a breaking change for the UI.

      if (details != null && 
        status.Equals(Hangfire.States.DeletedState.StateName, StringComparison.OrdinalIgnoreCase) && 
        details.Result.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        status = Hangfire.States.FailedState.StateName;
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
    [Route("internal/v1/background/{jobId}/result")]
    [HttpGet]
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
