using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;

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
    Task GetExportData(string url, IDictionary<string, string> customHeaders, PerformContext context);
  }
}
