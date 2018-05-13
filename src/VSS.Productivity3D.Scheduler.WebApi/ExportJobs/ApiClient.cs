using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Class for requesting data from a web api.
  /// </summary>
  public class ApiClient : IApiClient
  {
    private readonly IConfigurationStore configurationStore;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApiClient(IConfigurationStore configurationStore, ILoggerFactory logger)
    {
      log = logger.CreateLogger<ApiClient>();
      this.logger = logger;
      this.configurationStore = configurationStore;
    }

    /// <summary>
    /// Send an HTTP request to the requested URL
    /// </summary>
    /// <typeparam name="T">The type of data returned by the HTTP request</typeparam>
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <param name="streamPayload">Optional payload for POST requests</param>
    /// <returns>The result of the HTTP request as an istance of type T</returns>
    public async Task<T> SendRequest<T>(ScheduleJobRequest jobRequest, IDictionary<string, string> customHeaders, Stream streamPayload = null)
    {
      var method = jobRequest.Method ?? "GET";
      var result = default(T);
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        if (streamPayload != null)
          result = await request.ExecuteRequest<T>(jobRequest.Url, streamPayload, customHeaders, method);
        else
          result = await request.ExecuteRequest<T>(jobRequest.Url, method, customHeaders, jobRequest.Payload);
        log.LogDebug("Result of send request: {0}", result);
      }
      catch (Exception ex)
      {
        var message = ex.Message;
        var stacktrace = ex.StackTrace;
        //Check for 400 and 500 errors which come through as an inner exception
        if (ex.InnerException != null)
        {
          message = ex.InnerException.Message;
          stacktrace = ex.InnerException.StackTrace;
        }
        log.LogWarning("Error sending data: ", message);
        log.LogWarning("Stacktrace: ", stacktrace);
        throw;
      }
      return result;
    }
  }
}
