using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.MasterData.Models.Models;


namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for managing an export job.
  /// </summary>
  public class ExportJob : IExportJob
  {
    /// <summary>
    /// Used to store the final download link for export jobs
    /// </summary>
    public const string DownloadLinkStateKey = "downloadLink";

    /// <summary>
    /// Used to store the s3 key for the export jobs
    /// </summary>
    public const string S3KeyStateKey = "s3Key";

    /// <summary>
    /// Location to save incoming Scheduled Job Requests
    /// </summary>
    private const string S3ScheduleSaveLocation = "background";

    private readonly IApiClient apiClient;
    private readonly ITransferProxy transferProxy;
    private readonly ILogger log;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportJob(IApiClient apiClient, ITransferProxy transferProxy, ILoggerFactory logger)
    {
      log = logger.CreateLogger<ExportJob>();
      this.apiClient = apiClient;
      this.transferProxy = transferProxy;
    }

    /// <summary>
    /// Save the request in S3 for use in the background task, rather than in the Database
    /// </summary>
    /// <param name="request">Request to be saved</param>
    /// <returns>A Guid to be passed in to the background task</returns>
    private Guid SaveRequest(ScheduleJobRequest request)
    {
      var guid = Guid.NewGuid();
      var data = JsonConvert.SerializeObject(request, Formatting.None);
      var bytes = Encoding.UTF8.GetBytes(data);
      using (var ms = new MemoryStream(bytes))
      {
        transferProxy.Upload(ms, $"{S3ScheduleSaveLocation}/{guid}");
      }

      return guid;
    }

    /// <summary>
    /// Fetch the Schedule Job Request for a given Request ID
    /// </summary>
    /// <param name="requestId">Request ID returned from the SaveRequest Method</param>
    /// <returns>The original Scheduled Task class</returns>
    private async Task<ScheduleJobRequest> DownloadRequest(Guid requestId)
    {
      ScheduleJobRequest request = null;
      var fileStreamResult = await transferProxy.Download($"{S3ScheduleSaveLocation}/{requestId}");
      using (var ms = new MemoryStream())
      {
        fileStreamResult.FileStream.CopyTo(ms);
        var bytes = ms.ToArray();
        var data = Encoding.UTF8.GetString(bytes);
        request = JsonConvert.DeserializeObject<ScheduleJobRequest>(data);
      }

      return request;
    }

    /// <summary>
    /// Queue a Scheduled Job to be run in the background
    /// </summary>
    /// <param name="request">Scheduled Job Details</param>
    /// <param name="customHeaders">Any Customer headers to be passed with the Scheduled Job Request</param>
    /// <returns>A Job ID for the Background Job</returns>
    public string QueueJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders)
    {
      var savedRequestId = SaveRequest(request);

      // We have to pass in a null PerformContext, as Hangfire will inject the correct one when the job is run.
      var jobId = BackgroundJob.Enqueue(() => GetExportData(savedRequestId, customHeaders, null));

      return jobId;
    }

    /// <summary>
    /// Gets the export data from a web api and saves it to AWS S3.
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="context">Hangfire context</param>
    [ExportFailureFilter]
    [AutomaticRetry(Attempts = 0)]
    public async Task GetExportData(Guid requestId, IDictionary<string, string> customHeaders,
      PerformContext context)
    {
      // Refetch the Request Model from S3
      var request = await DownloadRequest(requestId);

      using (var data = await apiClient.SendRequest(request, customHeaders))
      {
        //TODO: Do we want something like applicationName/customerUid/userId/jobId for S3 path?
        //where app name and userId (appId or userUid) from JWT
        var stream = await data.ReadAsStreamAsync();
        var contentType = data.Headers.ContentType == null ? string.Empty : data.Headers.ContentType.MediaType;
        var path = GetS3Key(context.BackgroundJob.Id, request.Filename);

        if (string.IsNullOrEmpty(contentType))
        {
          // The default data will be zip file (for backwards compatability where it defaulted to zip files)
          path = path + ".zip"; 
          contentType = "application/octet-stream";
        }

        // Transfer proxy will upload the file with a potentially different extension, matching the contenttype
        // So we may get a new path
        var newPath = transferProxy.Upload(stream, path, contentType);
        
        // Set the results so the results can access the final url easily
        JobStorage.Current.GetConnection().SetJobParameter(context.BackgroundJob.Id, S3KeyStateKey, newPath);
        JobStorage.Current.GetConnection().SetJobParameter(context.BackgroundJob.Id, DownloadLinkStateKey, transferProxy.GeneratePreSignedUrl(newPath));
      }
    }

    /// <summary>
    /// Gets the download link for the completed job.
    /// </summary>
    [Obsolete("Use the JobStorage to store download links, as the requested filename could change")]
    public string GetDownloadLink(string jobId, string filename)
    {
      return transferProxy.GeneratePreSignedUrl(GetS3Key(jobId, filename));
    }

    /// <summary>
    /// Gets the S3 key for a job
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <param name="filename">The name of the file</param>
    /// <returns>The S3 key where the file is stored. This is the full path and file name in AWS.</returns>
    public static string GetS3Key(string jobId, string filename)
    {
      return $"3dpm/{jobId}/{filename}";
    }
  }
}
