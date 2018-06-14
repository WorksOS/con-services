using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
    /// Gets the export data from a web api and saves it to AWS S3.
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="context">Hangfire context</param>
    [ExportFailureFilter]
    [AutomaticRetry(Attempts = 0)]
    public async Task GetExportData(ScheduleJobRequest request, IDictionary<string, string> customHeaders,
      PerformContext context)
    {
      using (var data = await apiClient.SendRequest(request, customHeaders))
      {
        //TODO: Do we want something like applicationName/customerUid/userId/jobId for S3 path?
        //where app name and userId (appId or userUid) from JWT
        transferProxy.Upload(data, GetS3Key(context.BackgroundJob.Id, request.Filename));
      }
    }

    /// <summary>
    /// Gets the download link for the completed job.
    /// </summary>
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
      return $"3dpm/{jobId}/{filename}.zip";
    }
  }
}
