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

    private class RequestExecutor
    {
      private readonly string endpoint;
      private readonly string method;
      private readonly IDictionary<string, string> customHeaders;
      private readonly string payloadData;
      private readonly ILogger log;
      private const int BUFFER_MAX_SIZE = 1024;
      private readonly int logMaxChar;
      private readonly int? timeout;
      private Stream stream;

      private async Task<string> GetStringFromResponseStream(WebResponse response)
      {
        var readStream = response.GetResponseStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_MAX_SIZE);
        string responseString = string.Empty;

        try
        {
          Array.Clear(buffer, 0, buffer.Length);
          var read = await readStream.ReadAsync(buffer, 0, buffer.Length);
          responseString = Encoding.UTF8.GetString(buffer);
          responseString = responseString.Trim(Convert.ToChar(0));
          while (read > 0)
          {
            Array.Clear(buffer, 0, buffer.Length);
            read = await readStream.ReadAsync(buffer, 0, buffer.Length);
            responseString += Encoding.UTF8.GetString(buffer);
            responseString = responseString.Trim(Convert.ToChar(0));
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequest() T: InOddException {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          readStream?.Dispose();
          ArrayPool<byte>.Shared.Return(buffer);
          responseString = responseString.Trim(Convert.ToChar(0));
        }
        return responseString;
      }

      private async Task<StreamContent> GetStreamContentFromResponseStream(WebResponse response)
      {
        var readStream = response.GetResponseStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_MAX_SIZE);
        var resultStream = new MemoryStream();

        try
        {
          Array.Clear(buffer, 0, buffer.Length);
          var read = await readStream.ReadAsync(buffer, 0, buffer.Length);
          resultStream.Write(buffer, 0, read);
          while (read > 0)
          {
            Array.Clear(buffer, 0, buffer.Length);
            read = await readStream.ReadAsync(buffer, 0, buffer.Length);
            resultStream.Write(buffer, 0, read);
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequest() T: InOddException {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          readStream?.Dispose();
          ArrayPool<byte>.Shared.Return(buffer);
        }
        var result = new StreamContent(resultStream);

        // Don't set a content type if we don't know it (RFC-7231 Spec says)
        if (MediaTypeHeaderValue.TryParse(response.ContentType, out var contentType))
          result.Headers.ContentType = contentType;

        return result;
      }

      private async Task<WebRequest> PrepareWebRequest(string endpoint, string method,
        IDictionary<string, string> customHeaders, string payloadData = null, Stream requestStream = null, int? timeout = null)
      {
        var request = WebRequest.Create(endpoint);
        request.Method = method;
        if (timeout.HasValue)
        {
          request.Timeout = timeout.Value;
        }
        if (request is HttpWebRequest httpRequest)
        {
          httpRequest.Accept = "application/json";
          //Add custom headers e.g. JWT, CustomerUid, UserUid
          if (customHeaders != null)
          {
            foreach (var key in customHeaders.Keys)
            {
              if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
              {
                httpRequest.ContentType = customHeaders[key];
              }
              else if (string.Equals(key, "Accept", StringComparison.OrdinalIgnoreCase))
              {
                httpRequest.Accept = customHeaders[key];
              }
              else
              {
                httpRequest.Headers[key] = customHeaders[key];
              }
            }
          }
        }

        if (requestStream != null)
        {
          using (var writeStream = await request.GetRequestStreamAsync())
          {
            if (requestStream is MemoryStream stream)
            {
              var reqS = stream.ToArray();
              await writeStream.WriteAsync(reqS, 0, reqS.Length);
            }
            else
            {
              await requestStream.CopyToAsync(writeStream);
            }
          }
        }
        else
        {
          //Apply payload if any
          if (!string.IsNullOrEmpty(payloadData))
          {
            // don't overwrite any existing one.
            if (customHeaders == null || !customHeaders.ContainsKey("Content-Type"))
            {
              request.ContentType = "application/json";
            }
            //This fails to serialize the HttpWebRequest with a Json serialization exception for netcore 2.0
            //log.LogDebug($"PrepareWebRequest() T : requestWithPayload {JsonConvert.SerializeObject(request).Truncate(logMaxChar)}");

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(payloadData);
            request.ContentLength = bytes.Length;

            using (var writeStream = await request.GetRequestStreamAsync())
            {
              await writeStream.WriteAsync(bytes, 0, bytes.Length);
            }
          }
        }

        return request;
      }

      /// <summary>
      /// Request Executor with String Payload
      /// </summary>
      /// <param name="endpoint">Endpoint for the request</param>
      /// <param name="method">Method for the request</param>
      /// <param name="customHeaders">Any custom headers to be added to the request</param>
      /// <param name="payloadData">Any data to be used in the body</param>
      /// <param name="log">Logger</param>
      /// <param name="logMaxChar">Max chars to log</param>
      /// <param name="timeout">Timeout in milliseconds</param>
      public RequestExecutor(string endpoint, string method, IDictionary<string, string> customHeaders,
        string payloadData, ILogger log, int logMaxChar, int? timeout)
      {
        this.endpoint = endpoint;
        this.method = method;
        var comparer = StringComparer.OrdinalIgnoreCase;
        this.customHeaders = new Dictionary<string, string>(customHeaders, comparer);

        this.payloadData = payloadData;
        this.log = log;
        this.logMaxChar = logMaxChar;
        this.timeout = timeout;
      }

      /// <summary>
      /// Request Executor with Stream payload
      /// </summary>
      /// <param name="stream">Stream to data for the body content</param>
      public RequestExecutor(string endpoint, string method, IDictionary<string, string> customHeaders,
        Stream stream, ILogger log, int logMaxChar, int? timeout) : this(endpoint, method, customHeaders, string.Empty, log, logMaxChar, timeout)
      {
        this.stream = stream;
      }

      /// <summary>
      /// Execute the request and return a stream
      /// </summary>
      /// <returns>Stream to the response if successful, otherwise null</returns>
      public async Task<Stream> ExecuteActualStreamRequest()
      {
        var content = await ExecuteActualStreamContentRequest();
        if (content == null)
        {
          return null;
        }

        var stream = await content.ReadAsStreamAsync();
        if (stream != null)
        {
          stream.Position = 0;
        }

        return stream;
      }

      /// <summary>
      /// Execute the request and return a content stream
      /// </summary>
      /// <returns>StreamContent to the response if successful, otherwise null</returns>
      public async Task<StreamContent> ExecuteActualStreamContentRequest()
      {
        var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData, stream, timeout);

        WebResponse response = null;

        try
        {
          response = await request.GetResponseAsync();
          if (response != null)
          {
            log.LogDebug($"ExecuteRequest() T executed the request");
            return await GetStreamContentFromResponseStream(response);
          }
        }
        catch (WebException ex)
        {
          log.LogDebug($"ExecuteRequest() T: InWebException");

          using (var errorResponse = ex.Response)
          {
            if (errorResponse == null) throw;

            log.LogDebug("ExecuteRequestException() T: going to read stream");

            var responseString = await GetStringFromResponseStream(errorResponse);
            var httpResponse = (HttpWebResponse)errorResponse;

            log.LogDebug($"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode} responseString: {responseString.Truncate(logMaxChar)}");

            // Request Id may be required for TPAAS support issues.
            var tpassRequestId = errorResponse.Headers.Get("Tpaas-Request-Id");
            if (!string.IsNullOrEmpty(tpassRequestId))
            {
              log.LogDebug($"Tpaas-Request-Id: {tpassRequestId}");
            }

            throw new Exception($"{httpResponse.StatusCode} {responseString}");
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequestException() T: errorCode: {ex.Message}");

          if (ex.InnerException != null)
          {
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          }

          throw;
        }
        finally
        {
          response?.Dispose();
        }

        return null;
      }

      public async Task<T> ExecuteActualRequest<T>(Stream requestStream = null)
      {
        var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData, requestStream, timeout);

        string responseString = null;
        WebResponse response = null;

        try
        {
          log.LogDebug($"ExecuteRequest() T starting the request");
          response = await request.GetResponseAsync();
          if (response != null)
          {
            log.LogDebug($"ExecuteRequest() T executed the request");
            responseString = await GetStringFromResponseStream(response);
            log.LogDebug($"ExecuteRequest() T success: responseString {responseString.Truncate(logMaxChar)}");
          }
        }
        catch (WebException ex)
        {
          log.LogDebug($"ExecuteRequest() T: InWebException");

          using (var errorResponse = ex.Response)
          {
            if (errorResponse == null)
            {
              throw;
            }

            log.LogDebug("ExecuteRequestException() T: going to read stream");
            responseString = await GetStringFromResponseStream(errorResponse);

            var httpResponse = (HttpWebResponse)errorResponse;

            log.LogDebug($"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode} responseString: {responseString.Truncate(logMaxChar)}");

            // Request Id may be required for TPAAS support issues.
            var tpassRequestId = errorResponse.Headers.Get("Tpaas-Request-Id");
            if (!string.IsNullOrEmpty(tpassRequestId))
            {
              log.LogDebug($"Tpaas-Request-Id: {tpassRequestId}");
            }

            throw new Exception($"{httpResponse.StatusCode} {responseString}");
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequestException() T: errorCode: {ex.Message}");

          if (ex.InnerException != null)
          {
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          }

          throw;
        }
        finally
        {
          response?.Dispose();
        }

        if (!string.IsNullOrEmpty(responseString))
        {
          var responseObject = JsonConvert.DeserializeObject<T>(responseString);
          log.LogDebug($"ExecuteRequest() T. toReturn:{JsonConvert.SerializeObject(responseObject).Truncate(logMaxChar)}");

          return responseObject;
        }

        var defaultResponseObject = default(T);
        log.LogDebug($"ExecuteRequest() T. defaultToReturn:{JsonConvert.SerializeObject(defaultResponseObject).Truncate(logMaxChar)}");

        return defaultResponseObject;
      }
    }

    /// <summary>
    /// Executes the request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadData">The payload data.</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns></returns>
    public async Task<T> ExecuteRequest<T>(string endpoint, string method,
      IDictionary<string, string> customHeaders = null, string payloadData = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() T : endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))} payloadData {payloadData.Truncate(logMaxChar)}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(() =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var executor = new RequestExecutor(endpoint, method, customHeaders, payloadData, log, logMaxChar, timeout);
          return executor.ExecuteActualRequest<T>();
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            $"ExecuteRequest() T. exceptionToRethrow:{policyResult.FinalException} endpoint: {endpoint} method: {method}");
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
    /// <param name="method">The method.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <param name="payloadData">The payload data.</param>
    /// <param name="timeout">Optional timeout in millisecs for the request</param>
    /// <param name="retries">The retries.</param>
    /// <param name="suppressExceptionLogging">if set to <c>true</c> [suppress exception logging].</param>
    /// <returns>A Stream to the response if successful, otherwise null</returns>
    public async Task<Stream> ExecuteRequest(string endpoint, string method,
      IDictionary<string, string> customHeaders = null, string payloadData = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      var streamContent = await ExecuteRequestAsStreamContent(endpoint,
        method,
        customHeaders,
        payloadData,
        null,
        timeout,
        retries,
        suppressExceptionLogging);

      var stream = await streamContent.ReadAsStreamAsync();
      if (stream != null)
      {
        stream.Position = 0; // make sure we seek to the beginning, as the creeated stream may have a different position
      }

      return stream;
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
    public async Task<StreamContent> ExecuteRequestAsStreamContent(string endpoint, string method,
      IDictionary<string, string> customHeaders = null, string payloadData = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() Stream: endpoint {endpoint} " +
        $"method {method}, " +
        $"customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))} " +
        $"payloadData {payloadData.Truncate(logMaxChar)} " +
        $"has payloadStream: {payloadStream != null}, length: {payloadStream?.Length ?? 0}" );

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");

          var executor = payloadStream == null
            ? new RequestExecutor(endpoint, method, customHeaders, payloadData, log, logMaxChar, timeout)
            : new RequestExecutor(endpoint, method, customHeaders, payloadStream, log, logMaxChar, timeout);

          return await executor.ExecuteActualStreamContentRequest();
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
    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
      IDictionary<string, string> customHeaders = null, string method = "POST",
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug(
        $"ExecuteRequest() T(no method) : endpoint {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders).Truncate(logMaxChar))}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug($"Trying to execute request {endpoint}");
          var executor = new RequestExecutor(endpoint, method, customHeaders, string.Empty, log, logMaxChar, timeout);
          return await executor.ExecuteActualRequest<T>(payload);
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
