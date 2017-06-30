using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace MasterDataProxies
{

  public static class Extensions
  {
    private static readonly ConditionalWeakTable<object, RefId> _ids = new ConditionalWeakTable<object, RefId>();

    public static Guid GetRefId<T>(this T obj) where T : class
    {
      if (obj == null)
        return default(Guid);

      return _ids.GetOrCreateValue(obj).Id;
    }

    private class RefId
    {
      public Guid Id { get; } = Guid.NewGuid();
    }
  }

  public class GracefulWebRequest
  {
    private readonly ILogger log;

    public GracefulWebRequest(ILoggerFactory logger)
    {
      
      log = logger.CreateLogger<GracefulWebRequest>();
      log.LogDebug($"GracefulWebRequest: Constructor");
    }


    private class RequestExecutor<T>
    {
      private string endpoint;
      private string method;
      private IDictionary<string, string> customHeaders;
      private string payloadData;
      private readonly ILogger log;

      private string GetStringFromResponseStream(WebResponse response)
      {
        using (var readStream = response.GetResponseStream())
        {
          if (readStream != null)
          {
            log.LogDebug($"ReadStream: {readStream.GetRefId()}");
            using (var reader = new StreamReader(readStream, Encoding.UTF8))
            {
              var responseString = reader.ReadToEnd();
              return responseString;
            }
          }
          return string.Empty;
        }
      }

      private async Task<WebRequest> PrepareWebRequest(string endpoint, string method,
        IDictionary<string, string> customHeaders, string payloadData)
      {
        //log.LogDebug($"PrepareWebRequest: Requesting data from {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))}");

        var request = WebRequest.Create(endpoint);
        request.Method = method;
        if (request is HttpWebRequest)
        {
          var httpRequest = request as HttpWebRequest;
          httpRequest.Accept = "application/json";
          //Add custom headers e.g. JWT, CustomerUid, UserUid
          if (customHeaders != null)
          {
            foreach (var key in customHeaders.Keys)
            {
              if (key == "Content-Type")
              {
                httpRequest.ContentType = customHeaders[key];
              }
              else
              {
                httpRequest.Headers[key] = customHeaders[key];
              }
            }
          }
        }
        //Apply payload if any
        if (!String.IsNullOrEmpty(payloadData))
        {
          request.ContentType = "application/json";
          using (var writeStream = await request.GetRequestStreamAsync())
          {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(payloadData);
            await writeStream.WriteAsync(bytes, 0, bytes.Length);
          }
        }
        return request;
      }

      public RequestExecutor(string endpoint, string method, IDictionary<string, string> customHeaders,
        string payloadData, ILogger log)
      {
        log.LogDebug("In contrusctor");
        this.endpoint = endpoint;
        this.method = method;
        this.customHeaders = customHeaders;
        this.payloadData = payloadData;
        this.log = log;
      }



      public async Task<T> ExecuteActualRequest()
      {
        log.LogDebug("Preparing request");
        var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);
        log.LogDebug($"Request: {request.GetRefId()}");


        string responseString = null;
        WebResponse mainRresponse = null;
        try
        {
          mainRresponse = await request.GetResponseAsync();
          log.LogDebug($"Response: {mainRresponse.GetRefId()}");
          if (mainRresponse != null)
          {
            log.LogDebug($"ExecuteRequest() T executed the request");
            log.LogDebug($"Content Length: {mainRresponse.ContentLength}");

            var readStream = mainRresponse.GetResponseStream();
            log.LogDebug($"ReadStream: {readStream.GetRefId()}");
            var reader = new StreamReader(readStream, Encoding.UTF8);
            responseString = reader.ReadToEnd();
            log.LogDebug($"ExecuteRequest() T success: responseString {responseString}");
          }
        }
        catch (WebException ex)
        {
          log.LogDebug($"ExecuteRequest() T: InWebException");
          using (WebResponse response = ex.Response)
          {
            if (response == null) throw;
            log.LogDebug("ExecuteRequestException() T: going to read stream");
            responseString = GetStringFromResponseStream(response);
            HttpWebResponse httpResponse = (HttpWebResponse) response;
            log.LogDebug(
              $"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode} responseString: {responseString}");
            throw new Exception($"{httpResponse.StatusCode} {responseString}");
          }
        }
        catch (Exception ex)
        {
          log.LogDebug($"ExecuteRequestException() T: errorCode: {ex.Message}");
          if (ex.InnerException != null)
            log.LogDebug($"ExecuteRequestInnerException() T: errorCode: {ex.InnerException.Message}");
          throw;
        }
        finally
        {
          mainRresponse?.Dispose();
        }

        if (!string.IsNullOrEmpty(responseString))
        {
          var toReturn = JsonConvert.DeserializeObject<T>(responseString);
          log.LogDebug($"ExecuteRequest() T. toReturn:{JsonConvert.SerializeObject(toReturn)}");
          return toReturn;
        }
        var defaultToReturn = default(T);
        log.LogDebug($"ExecuteRequest() T. defaultToReturn:{JsonConvert.SerializeObject(defaultToReturn)}");
        return defaultToReturn;

      }
    }

    public async Task<T> ExecuteRequest<T>(string endpoint, string method, IDictionary<string, string> customHeaders = null, 
      string payloadData = null, int retries = 3, bool suppressExceptionLogging=false)
    {
      log.LogDebug($"ExecuteRequest() T : endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");
    
      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          log.LogDebug("Trying to execute request");
          var executor = new RequestExecutor<T>(endpoint, method, customHeaders, payloadData,log);
          return await executor.ExecuteActualRequest();
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug($"ExecuteRequest() T. exceptionToRethrow:{policyResult.FinalException} endpoint: {endpoint} method: {method}");
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return default(T);
    }


    public async Task<Stream> ExecuteRequest(string endpoint, string method, IDictionary<string, string> customHeaders = null,
      string payloadData = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug($"ExecuteRequest() Stream: endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

          Stream responseStream = null;
          try
          {
            using (WebResponse response = await request.GetResponseAsync())
            {
              responseStream = GetStreamFromResponse(response);
              log.LogDebug($"ExecuteRequest() stream success");
            }
          }
          catch (Exception ex)
          {
            log.LogDebug($"ExecuteRequestException() stream: errorCode: {ex.Message}");
            throw;
          }
          return responseStream;
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug($"ExecuteRequest() Stream: exceptionToRethrow:{policyResult.FinalException.ToString()} endpoint: {endpoint} method: {method}");
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return null;
    }


    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
      IDictionary<string, string> customHeaders = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug($"ExecuteRequest() T(no method) : endpoint {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))}");

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          var request = WebRequest.Create(endpoint);
          request.Method = "POST";
          if (request is HttpWebRequest)
          {
            var httpRequest = request as HttpWebRequest;
            httpRequest.Accept = "*/*";
            //Add custom headers e.g. JWT, CustomerUid, UserUid
            if (customHeaders != null)
            {
              foreach (var key in customHeaders.Keys)
              {
                if (key == "Content-Type")
                {
                  httpRequest.ContentType = customHeaders[key];
                }
                else
                {
                  httpRequest.Headers[key] = customHeaders[key];
                }
              }
            }
          }

          using (var writeStream = await request.GetRequestStreamAsync())
          {
            await payload.CopyToAsync(writeStream);
          }

          string responseString = null;
          using (var response = await request.GetResponseAsync())
          {
            responseString = GetStringFromResponseStream(response);
          }
          if (!string.IsNullOrEmpty(responseString))
            return JsonConvert.DeserializeObject<T>(responseString);
          return default(T);
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


    private async Task<WebRequest> PrepareWebRequest(string endpoint, string method,
      IDictionary<string, string> customHeaders, string payloadData)
    {
      //log.LogDebug($"PrepareWebRequest: Requesting data from {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))}");

      var request = WebRequest.Create(endpoint);
      request.Method = method;
      if (request is HttpWebRequest)
      {
        var httpRequest = request as HttpWebRequest;
        httpRequest.Accept = "application/json";
        //Add custom headers e.g. JWT, CustomerUid, UserUid
        if (customHeaders != null)
        {
          foreach (var key in customHeaders.Keys)
          {
            if (key == "Content-Type")
            {
              httpRequest.ContentType = customHeaders[key];
            }
            else
            {
              httpRequest.Headers[key] = customHeaders[key];
            }
          }
        }
      }
      //Apply payload if any
      if (!String.IsNullOrEmpty(payloadData))
      {
        request.ContentType = "application/json";
        using (var writeStream = await request.GetRequestStreamAsync())
        {
          UTF8Encoding encoding = new UTF8Encoding();
          byte[] bytes = encoding.GetBytes(payloadData);
          await writeStream.WriteAsync(bytes, 0, bytes.Length);
        }
      }
      return request;
    }

    private string GetStringFromResponseStream(WebResponse response)
    {
      using (var readStream = response.GetResponseStream())
      {
        if (readStream != null)
        {
          using (var reader = new StreamReader(readStream, Encoding.UTF8))
          {
            var responseString = reader.ReadToEnd();
            return responseString;
          }
        }
        return string.Empty;
      }
    }

    private Stream GetStreamFromResponse(WebResponse response)
    {
      using (var readStream = response.GetResponseStream())
      {
        var streamFromResponse = new MemoryStream();

        if (readStream != null)
        {
          readStream.CopyTo(streamFromResponse);
          return streamFromResponse;
        }
        return null;
      }
    }
    
  }
}
