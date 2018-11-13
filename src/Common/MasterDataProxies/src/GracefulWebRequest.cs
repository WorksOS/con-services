using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using VSS.ConfigurationStore;

namespace VSS.MasterData.Proxies
{
  public class GracefulWebRequest
  {
    /// <summary>
    /// If there is no provided LOG_MAX_CHAR env variable, then we will default to this
    /// </summary>
    private const int DefaultLogMaxChar = 1000;

    private readonly ILogger log;
    private readonly int logMaxChar;

    public GracefulWebRequest(ILoggerFactory logger, IConfigurationStore configStore)
    {
      log = logger.CreateLogger<GracefulWebRequest>();
      logMaxChar = configStore.GetValueInt("LOG_MAX_CHAR");

      // Config Store may return -1 if the variable doesn't exist
      if (logMaxChar <= 0)
      {
        log.LogInformation($"Missing environment variable LOG_MAX_CHAR, defaulting to {DefaultLogMaxChar}");
        logMaxChar = DefaultLogMaxChar;
      }
    }


    private async Task<(HttpStatusCode, Stream)> ExecuteRequestInternal(string endpoint, string method,
      IDictionary<string, string> customHeaders, Stream requestStream = null, int? timeout = null)
    {
      var client = new HttpClient();
      if (customHeaders != null)
      {
        foreach (var customHeader in customHeaders)
         // if (customHeader.Key != "Content-Type")
           if (!client.DefaultRequestHeaders.TryAddWithoutValidation(customHeader.Key, customHeader.Value))
             log.LogWarning($"Can't add header {customHeader.Key}");
      }

     /* var contentType = string.Empty;
      var data = string.Empty;
      HttpContent content = null;
      if (customHeaders.ContainsKey("Content-Type"))
      {
        contentType = customHeaders["Content-Type"];
        data = new StreamReader(requestStream).ReadToEnd();
        content = new StringContent(data, Encoding.UTF8, contentType);
      }
      else
      {
        content = new StreamContent(requestStream);
      }*/

      HttpResponseMessage response;
      switch (method)
      {
        case "GET":
        {
          response = await client.GetAsync(endpoint);
          break;
        }
        case "POST":
        {
          response = await client.PostAsync(endpoint, new StreamContent(requestStream));
          break;
        }
        case "PUT":
        {
          response = await client.PutAsync(endpoint, new StreamContent(requestStream));
          break;
        }
        default:
        {
          throw new ArgumentException("Unknown HTTP method");
        }
      }

      return (response.StatusCode, await response.Content.ReadAsStreamAsync());
    }



    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadData">The payload data.</param>
    /// <param name="payloadStream">Stream to payload data, will be used instead of payloadData if set</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns>A stream content representing the result returned from the endpoint if successful, otherwise null</returns>
    public async Task<Stream> ExecuteRequestAsStreamContent(string endpoint, string method,
      IDictionary<string, string> customHeaders = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() Stream: endpoint {endpoint} " +
        $"method {method}, " +
        $"customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))} " +
        $"has payloadStream: {payloadStream != null}, length: {payloadStream?.Length ?? 0}" );

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");

          log.LogDebug($"Trying to execute request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payloadStream, timeout);

          if (result.Item1 != HttpStatusCode.OK)
          {
            var contents = (new StreamReader(result.Item2)).ReadToEnd();
            throw new HttpRequestException($"Request returned non-ok code {result.Item1} with response {contents}");
          }

          return result.Item2;
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            $"ExecuteRequest() Stream: exceptionToRethrow:{policyResult.FinalException.ToString()} endpoint: {endpoint} method: {method}");
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return null;
    }

    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="method">The method.</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns></returns>
    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload = null,
      IDictionary<string, string> customHeaders = null, string method = "POST",
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() T({method}) : endpoint {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))}");

      if (payload==null && method!="GET")
        throw new ArgumentException("Can't have null payload with a non-GET method.");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payload, timeout);

          var contents = (new StreamReader(result.Item2)).ReadToEnd();
          if (result.Item1 != HttpStatusCode.OK)
          {
            log.LogDebug($"Request returned non-ok code {result.Item1} with response {contents}");
            throw new HttpRequestException($"Request returned non-ok code {result.Item1} with response {contents}");
          }
          log.LogDebug($"Request returned {contents.Truncate(logMaxChar)} with status {result.Item1}");
          if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(contents,typeof(T));
          return JsonConvert.DeserializeObject<T>(contents);
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            "ExecuteRequest_multi(). exceptionToRethrow:{0} endpoint: {1} customHeaders: {2}",
            policyResult.FinalException.ToString(), endpoint, customHeaders);
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return default(T);
    }

  }
}
