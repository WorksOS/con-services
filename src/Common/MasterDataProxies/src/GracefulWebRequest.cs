using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class GracefulWebRequest : IWebRequest
  {
    private readonly ILogger _log;
    private const int DefaultLogMaxChar = 1000;
    private readonly int _logMaxChar;

    static readonly HttpClientHandler handler = new HttpClientHandler()
    {
      AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    };

    ///TODO since our apps is a mix of netcore 2.0, netcore 2.1 and net 4.7.1 this should be replaced with httpclient factory once all services are using the same target
    private static readonly HttpClient httpClient = new HttpClient(handler) {Timeout = TimeSpan.FromMinutes(30)};

    //Any 200 code is ok.
    private static readonly List<HttpStatusCode> okCodes = new List<HttpStatusCode>
    { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.NonAuthoritativeInformation,
      HttpStatusCode.NoContent, HttpStatusCode.ResetContent, HttpStatusCode.PartialContent};

    public GracefulWebRequest(ILoggerFactory logger, IConfigurationStore configStore)
    {
      _log = logger.CreateLogger<GracefulWebRequest>();
      _logMaxChar = configStore.GetValueInt("LOG_MAX_CHAR", DefaultLogMaxChar);
    }


    private Task<HttpResponseMessage> ExecuteRequestInternal(string endpoint, HttpMethod method,
      IDictionary<string, string> customHeaders, Stream requestStream = null, int? timeout = null)
    {
      void ApplyHeaders(IDictionary<string, string> dictionary, HttpRequestMessage x)
      {
        if (dictionary != null)
        {
          foreach (var customHeader in dictionary)
            if (!x.Headers.TryAddWithoutValidation(customHeader.Key, customHeader.Value))
              _log.LogWarning($"Can't add header {customHeader.Key}");
        }

        if (!x.Headers.Contains("Accept"))
        {
          if (!x.Headers.TryAddWithoutValidation("Accept", "*/*"))
            _log.LogWarning("Can't add Accept header");
        }
      }

      // If we retry a request that uses a stream payload, it will not reset the position to 0
      // Causing an empty body to be sent (which is invalid for POST requests).
      if (requestStream != null && requestStream.CanSeek)
        requestStream.Seek(0, SeekOrigin.Begin);

      if (method == HttpMethod.Get)
        return httpClient.GetAsync(endpoint, timeout, x => { ApplyHeaders(customHeaders, x); }, _log);

      if (method == HttpMethod.Post || method == HttpMethod.Put)
        return httpClient.PostAsync(endpoint, requestStream, method, customHeaders, timeout,
          x => { ApplyHeaders(customHeaders, x); }, _log);

      if (method == HttpMethod.Delete)
        return httpClient.DeleteAsync(endpoint, timeout, x => { ApplyHeaders(customHeaders, x); }, _log);

      throw new ArgumentException($"Unknown HTTP method {method}");
    }



    /// <summary>
    /// Execute a request
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadStream">Stream to payload data, will be used instead of payloadData if set</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <exception cref="ArgumentException">If the Method is POST/PUT and the Payload is null.</exception>
    /// <exception cref="HttpRequestException">If the Status Code from the request is not 200.</exception>
    /// <returns>A stream content representing the result returned from the endpoint if successful, otherwise null</returns>
    public async Task<HttpContent> ExecuteRequestAsStreamContent(string endpoint, HttpMethod method,
      IDictionary<string, string> customHeaders = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      _log.LogDebug(
        $"ExecuteRequest() Stream: endpoint {endpoint} " +
        $"method {method}, " +
        $"customHeaders {customHeaders.LogHeaders()} " +
        $"has payloadStream: {payloadStream != null}, length: {payloadStream?.Length ?? 0}");

      // We can't retry if we get a stream that doesn't support seeking (should be rare, but handle it incase)
      if (payloadStream != null && !payloadStream.CanSeek && retries > 0)
      {
        _log.LogWarning(
          $"Attempting a HTTP {method} with a Stream ({payloadStream.GetType().Name}) that doesn't not support seeking, disabling retries");
        retries = 0; 
      }

      var policyResult = await Policy
        .Handle<Exception>(exception =>
        {
          _log.LogWarning($"Polly failed to execute the request {endpoint} with exception {exception.Message}");
          return true;
        })
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          _log.LogDebug($"Trying to execute {method} request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payloadStream, timeout);
          _log.LogDebug($"Request to {endpoint} completed with statuscode {result.StatusCode} and content length {result.Content.Headers.ContentLength}");

          if (!okCodes.Contains(result.StatusCode))
          {
            var contents = await result.Content.ReadAsStringAsync();
            var serviceException = ParseServiceError(result.StatusCode, contents);
            // The contents will contain a message from the end point s
            throw new HttpRequestException($"{result.StatusCode} {contents}", serviceException);
          }

          return result.Content;
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          _log.LogDebug(
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
    /// Executes the request. (Only use for classes that can be Serialized to JSON, i.e not binary data unless it's BASE64 Encoded)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="method">The method.</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <exception cref="ArgumentException">If the Method is POST/PUT and the Payload is null.</exception>
    /// <exception cref="HttpRequestException">If the Status Code from the request is not 200.</exception>
    /// <returns></returns>
    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload = null,
      IDictionary<string, string> customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      // Default to POST
      if (method == null)
        method = HttpMethod.Post;

      _log.LogDebug(
        $"ExecuteRequest() T({method}) : endpoint {endpoint} customHeaders {customHeaders.LogHeaders()}");

      // We can't retry if we get a stream that doesn't support seeking (should be rare, but handle it incase)
      if (payload != null && !payload.CanSeek && retries > 0)
      {
        _log.LogWarning(
          $"Attempting a HTTP {method} with a Stream ({payload.GetType().Name}) that doesn't not support seeking, disabling retries");
        retries = 0;
      }

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          _log.LogDebug($"Trying to execute {method} request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payload, timeout);
          _log.LogDebug($"Request to {endpoint} completed");

          var contents = await result.Content.ReadAsStringAsync();
          if (!okCodes.Contains(result.StatusCode))
          {
            _log.LogDebug($"Request returned non-ok code {result.StatusCode} with response {contents}");
           
            var serviceException = ParseServiceError(result.StatusCode, contents);
            throw new HttpRequestException($"{result.StatusCode} {contents}", serviceException);
          }

          _log.LogDebug($"Request returned {contents.Truncate(_logMaxChar)} with status {result.StatusCode}");
          if (typeof(T) == typeof(string)) return (T) Convert.ChangeType(contents, typeof(T));
          return JsonConvert.DeserializeObject<T>(contents);
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          _log.LogDebug(
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

    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="method">The method.</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <exception cref="ArgumentException">If the Method is POST/PUT and the Payload is null.</exception>
    /// <exception cref="HttpRequestException">If the Status Code from the request is not any code in the 200's.</exception>
    /// <returns></returns>
    public async Task ExecuteRequest(string endpoint, Stream payload = null,
      IDictionary<string, string> customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      //Default to POST
      if (method == null)
        method = HttpMethod.Post;

      _log.LogDebug(
        $"ExecuteRequest() ({method}) : endpoint {endpoint} customHeaders {customHeaders.LogHeaders()}");

      // We can't retry if we get a stream that doesn't support seeking (should be rare, but handle it incase)
      if (payload != null && !payload.CanSeek && retries > 0)
      {
        _log.LogWarning(
          $"Attempting a HTTP {method} with a Stream ({payload.GetType().Name}) that doesn't not support seeking, disabling retries");
        retries = 0;
      }

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          _log.LogDebug($"Trying to execute {method} request {endpoint}");
          var result = await ExecuteRequestInternal(endpoint, method, customHeaders, payload, timeout);
          _log.LogDebug($"Request to {endpoint} completed");

          if (!okCodes.Contains(result.StatusCode))
          {
            var contents = await result.Content.ReadAsStringAsync();
            _log.LogDebug($"Request returned non-ok code {result.StatusCode} with response {contents}");
            var serviceException = ParseServiceError(result.StatusCode, contents);
            throw new HttpRequestException($"{result.StatusCode} {contents}", serviceException);
          }

          _log.LogDebug($"Request returned status {result.StatusCode}");
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          _log.LogDebug(
            "ExecuteRequest_multi(). exceptionToRethrow:{0} endpoint: {1} customHeaders: {2}",
            policyResult.FinalException.ToString(), endpoint, customHeaders);
        }

        throw policyResult.FinalException;
      }
    }

    /// <summary>
    /// Attempt to parse the result body, and convert to a service exception
    /// Returns null if not in the correct format (will not throw a new exception)
    /// </summary>
    /// <returns>Service Exception if in the correct format, else null</returns>
    private static ServiceException ParseServiceError(HttpStatusCode code, string contents)
    {
      ServiceException serviceException = null;
      // Attempt to parse the service exception result
      try
      {
        var serviceExecutionResult = JsonConvert.DeserializeObject<ContractExecutionResult>(contents);
        serviceException = new ServiceException(code, serviceExecutionResult);
      }
      catch
      {
        // ignored
        // Not the type we wanted, move along
        serviceException = null;
      }

      return serviceException;
    }
  }
}
