using System.Collections.Generic;
using System.IO;
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
    /// <typeparam name="T">The type of data returned by the HTTP request</typeparam>
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <param name="streamPayload">Optional payload for POST requests</param>
    /// <returns>The result of the HTTP request as an istance of type T</returns>
    Task<T> SendRequest<T>(ScheduleJobRequest jobRequest, IDictionary<string, string> customHeaders,
      Stream streamPayload = null);
  }
}
