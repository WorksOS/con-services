using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Interface for API calls to requested URLs
  /// </summary>
  public interface IApiClient
  {
    /// <summary>
    /// Send an HTTP request to the requested URL
    /// </summary>
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <returns>The result of the HTTP request as stream</returns>
    Task<StreamContent> SendRequest(ScheduleJobRequest jobRequest, IDictionary<string, string> customHeaders);
  }
}
