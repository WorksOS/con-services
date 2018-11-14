using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
    /// <param name="jobRequest">Details of the job request</param>
    /// <param name="customHeaders">Custom HTTP headers for the HTTP request</param>
    /// <returns>The result of the HTTP request as a stream</returns>
    public async Task<HttpContent> SendRequest(ScheduleJobRequest jobRequest, IDictionary<string, string> customHeaders)
    {
      HttpContent result = null;
      var method = jobRequest.Method ?? "GET";
      log.LogDebug($"Job request is {jobRequest}");
      try
      {
        var request = new GracefulWebRequest(logger, configurationStore);
        // Merge the Custom headers passed in with the http request, and the headers requested by the Schedule Job
        foreach (var header in jobRequest.Headers)
        {
          if (!customHeaders.ContainsKey(header.Key))
          {
            customHeaders[header.Key] = header.Value;
          }
          else
          {
            log.LogDebug($"HTTP Header '{header.Key}' exists in both the web requests and job request headers, using web request value. Web Request Value: '${customHeaders[header.Key]}', Job Request Value: '${header.Value}'");
          }
        }
        
        // The Schedule job request may contain encoded binary data, or a standard string,
        // We need to handle both cases differently, as we could lose data if converting binary information to a string
        if (jobRequest.IsBinaryData)
        {
          using (var ms = new MemoryStream(jobRequest.PayloadBytes))
          {
            result = await request.ExecuteRequestAsStreamContent(jobRequest.Url, method, customHeaders, ms,
              jobRequest.Timeout, 0);
          }
        }
        else
        {
          result = await request.ExecuteRequestAsStreamContent(jobRequest.Url, method, customHeaders,
            new MemoryStream(Encoding.UTF8.GetBytes(jobRequest.Payload)), jobRequest.Timeout, 0);
        }

        log.LogDebug("Result of send request: Stream Content={0}", result);
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
