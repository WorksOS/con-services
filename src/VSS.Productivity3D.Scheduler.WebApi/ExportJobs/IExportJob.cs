using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
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
    Task GetExportData(ScheduleJobRequest request, IDictionary<string, string> customHeaders, PerformContext context);

    /// <summary>
    /// Gets the download link for the completed job
    /// </summary>
    string GetDownloadLink(string jobId, string filename);
  }
}
