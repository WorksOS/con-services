using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Export job handler interface.
  /// </summary>
  public interface IExportJob
  {
    /// <summary>
    /// Gets export data from specified url.
    /// </summary>
    Task GetExportData(Guid requestId, IHeaderDictionary customHeaders, PerformContext context);

    /// <summary>
    /// Queue a Scheduled Job to be run in the background
    /// </summary>
    /// <param name="request">Scheduled Job Details</param>
    /// <param name="customHeaders">Any Customer headers to be passed with the Scheduled Job Request</param>
    /// <returns>A Job ID for the Background Job</returns>
    string QueueJob(ScheduleJobRequest request, IHeaderDictionary customHeaders);

    /// <summary>
    /// Gets the download link for the completed job
    /// </summary>
    string GetDownloadLink(string jobId, string filename);
  }
}
