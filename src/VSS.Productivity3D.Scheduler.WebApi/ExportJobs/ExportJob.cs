using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire.Server;
using VSS.MasterData.Models.Local.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for managing an export job.
  /// </summary>
  public class ExportJob : IExportJob
  {
    private readonly IApiClient apiClient;
    private readonly ITransferProxy transferProxy;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public ExportJob(IApiClient apiClient, ITransferProxy transferProxy)
    {
      this.apiClient = apiClient;
      this.transferProxy = transferProxy;
    }

    /// <summary>
    /// Gets the export data from a web api and saves it to AWS S3.
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="context">Hangfire context</param>
    public async Task GetExportData(ScheduleJobRequest request, IDictionary<string, string> customHeaders,
      PerformContext context)
    {
      //TODO: Do we want the type returned to be generic? i.e. ExportResult passed here as T.
      //But then how do we know how to save the file to S3?
      var data = await apiClient.SendRequest<ExportResult>(request, customHeaders);
      //TODO: Do we want something like applicationName/customerUid/userId/jobId for S3 path?
      //where app name and userId (appId or userUid) from JWT
      transferProxy.Upload(new MemoryStream(data.ExportData), GetS3Key(context.BackgroundJob.Id));
    }

    /// <summary>
    /// Gets the download link for the completed job.
    /// </summary>
    public string GetDownloadLink(string jobId)
    {
      return transferProxy.GeneratePreSignedUrl(GetS3Key(jobId));
    }

    /// <summary>
    /// Gets the S3 key for a job
    /// </summary>
    /// <param name="jobId">The job id</param>
    /// <returns>The S3 key where the file is stored. This is the full path and file name in AWS.</returns>
    public static string GetS3Key(string jobId)
    {
      return $"3dpm/{jobId}.zip";
    }
  }
}
