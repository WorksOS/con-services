using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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

    ///TODO since our apps is a mix of netcore 2.0, netcore 2.1 and net 4.7.1 this should be replaced with httpclient factory once all services are using the same target
    private static readonly HttpClient httpClient = new HttpClient();

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


    private Task<HttpResponseMessage> ExecuteRequestInternal(string endpoint, string method,
      IDictionary<string, string> customHeaders, Stream requestStream = null, int? timeout = null)
    {
      void ApplyHeaders(IDictionary<string, string> dictionary, HttpRequestMessage x)
      {
        if (dictionary != null)
        {
          foreach (var customHeader in dictionary)
            if (!x.Headers.TryAddWithoutValidation(customHeader.Key, customHeader.Value))
              log.LogWarning($"Can't add header {customHeader.Key}");
        }
      }

      if (requestStream == null && method != "GET")
        throw new ArgumentException($"Empty body for non-GET request {nameof(requestStream)}");

      //We need to disable this due to concurrency issues as it is not supported within a single httpclient instance
      /*if (timeout.HasValue)
        httpClient.Timeout = TimeSpan.FromSeconds(timeout.Value);*/

      log.LogDebug(
        $"Headers to be attached to the request {JsonConvert.SerializeObject(httpClient.DefaultRequestHeaders)}");

      switch (method)
      {
        case "GET":
        {
          return httpClient.GetAsync(endpoint, timeout, x => { ApplyHeaders(customHeaders, x); });
        }
        case "POST":
        case "PUT":
        {
          return httpClient.PostAsync(endpoint, requestStream, method, customHeaders, timeout,
            x => { ApplyHeaders(customHeaders, x); });
        }
        default:
        {
          throw new ArgumentException($"Unknown HTTP method {nameof(method)}");
        }
      }
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
    public async Task<HttpContent> ExecuteRequestAsStreamContent(string endpoint, string method,
      IDictionary<string, string> customHeaders = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() Stream: endpoint {endpoint} " +
        $"method {method}, " +
        $"customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))} " +
        $"has payloadStream: {payloadStream != null}, length: {payloadStream?.Length ?? 0}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");

          log.LogDebug($"Trying to execute request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payloadStream, timeout);

          if (result.StatusCode != HttpStatusCode.OK)
          {
            var contents = await result.Content.ReadAsStringAsync();

            // The contents will contain a message from the end point s
            throw new HttpRequestException($"{result.StatusCode} {contents}");
          }

          return result.Content;
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

      if (payload == null && method != "GET")
        throw new ArgumentException("Can't have null payload with a non-GET method.");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payload, timeout);

          var contents = await result.Content.ReadAsStringAsync();
          if (result.StatusCode != HttpStatusCode.OK)
          {
            log.LogDebug($"Request returned non-ok code {result.StatusCode} with response {contents}");
            throw new HttpRequestException($"{result.StatusCode} {contents}");
          }

          log.LogDebug($"Request returned {contents.Truncate(logMaxChar)} with status {result.StatusCode}");
          if (typeof(T) == typeof(string)) return (T) Convert.ChangeType(contents, typeof(T));
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

  internal static class HttpClientExtensions
  {
    public static Task<HttpResponseMessage> GetAsync
      (this HttpClient httpClient, string uri, int? timeout, Action<HttpRequestMessage> preAction)
    {
      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

      preAction(httpRequestMessage);

      var cts = new CancellationTokenSource(timeout.HasValue?timeout.Value:60000);

      return httpClient.SendAsync(httpRequestMessage,cts.Token);
    }

    public static Task<HttpResponseMessage> PostAsync
    (this HttpClient httpClient, string uri, Stream requestStream, string method,
      IDictionary<string, string> customHeaders, int? timeout, Action<HttpRequestMessage> preAction)
    {
      //Default to JSON content type
      HttpContent content = null;
      if (requestStream != null)
      {
        if ((customHeaders == null) || !customHeaders.ContainsKey("Content-Type") ||
            (customHeaders.ContainsKey("Content-Type") &&
             customHeaders["Content-Type"] == "application/json"))
        {
          content = new StringContent(new StreamReader(requestStream).ReadToEnd(), Encoding.UTF8, "application/json");
        }
        else
        {
          content = new StreamContent(requestStream);
        }
      }

      //Default to POST, nothing else is supported so far
      var httpMethod = HttpMethod.Post;
      if (method == "PUT")
        httpMethod = HttpMethod.Put;
      var httpRequestMessage = new HttpRequestMessage(httpMethod, uri)
      {
        Content = content
      };
      preAction(httpRequestMessage);

      var cts = new CancellationTokenSource(timeout.HasValue ? timeout.Value : 60000);

      return httpClient.SendAsync(httpRequestMessage,cts.Token);
    }
  }
}
