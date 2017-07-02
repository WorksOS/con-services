using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace MasterDataProxies
{
  public class GracefulWebRequest
  {
    private readonly ILogger log;

    public GracefulWebRequest(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GracefulWebRequest>();
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
          var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

          string responseString = null;
          try
          {
            using (WebResponse response = await request.GetResponseAsync())
            {
              if (response != null)
              {
                responseString = GetStringFromResponseStream(response);
                log.LogDebug($"ExecuteRequest() T success: responseString{responseString}");
              }
            }
          }
          catch (WebException ex)
          {
            using (WebResponse response = ex.Response)
            {
              if (response == null) throw;
              responseString = GetStringFromResponseStream(response);
              HttpWebResponse httpResponse = (HttpWebResponse) response;
              log.LogDebug(
                $"ExecuteRequestException() T: errorCode: {httpResponse.StatusCode} responseString: {responseString}");
              throw new Exception($"{httpResponse.StatusCode} {responseString}");
            }
          }
          catch (Exception ex)
          {
            log.LogDebug(
              $"ExecuteRequestException() T: errorCode: {ex.Message}");
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


    //public async Task<Stream> ExecuteRequest(string endpoint, string method, IDictionary<string, string> customHeaders = null,
    //  string payloadData = null, int retries = 3, bool suppressExceptionLogging = false)
    //{
    //  log.LogDebug($"ExecuteRequest() Stream: endpoint {endpoint} method {method}, customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");

    //  var policyResult = await Policy
    //    .Handle<Exception>()
    //    .RetryAsync(retries)
    //    .ExecuteAndCaptureAsync(async () =>
    //    {
    //      var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

    //      Stream responseStream = null;
    //      try
    //      {
    //        using (WebResponse response = await request.GetResponseAsync())
    //        {
    //          responseStream = GetStreamFromResponse(response);
    //        }
    //      }
    //      catch (WebException ex)
    //      {
    //        using (WebResponse response = ex.Response)
    //        {
    //          if (response == null) throw;
    //          var responseString = GetStringFromResponseStream(response);
    //          HttpWebResponse httpResponse = (HttpWebResponse)response;
    //          log.LogDebug($"ExecuteRequestException()  Stream: errorCode: {httpResponse.StatusCode} responseString: {responseString}");
    //          throw new Exception($"{httpResponse.StatusCode} {responseString}");
    //        }
    //      }
    //      return responseStream;
    //    });

    //  if (policyResult.FinalException != null)
    //  {
    //    if (!suppressExceptionLogging)
    //    {
    //      log.LogDebug($"ExecuteRequest() Stream: exceptionToRethrow:{policyResult.FinalException.ToString()} endpoint: {endpoint} method: {method}");
    //    }
    //    throw policyResult.FinalException;
    //  }
    //  if (policyResult.Outcome == OutcomeType.Successful)
    //  {
    //    return policyResult.Result;
    //  }
    //  return null;
    //}


    //public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
    //  IDictionary<string, string> customHeaders = null, int retries = 3, bool suppressExceptionLogging = false)
    //{
    //  log.LogDebug($"ExecuteRequest() T(no method) : endpoint {endpoint} customHeaders {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))} payloadData {payloadData}");
      
    //  var policyResult = await Policy
    //    .Handle<Exception>()
    //    .RetryAsync(retries)
    //    .ExecuteAndCaptureAsync(async () =>
    //    {
    //      var request = WebRequest.Create(endpoint);
    //      request.Method = "POST";
    //      if (request is HttpWebRequest)
    //      {
    //        var httpRequest = request as HttpWebRequest;
    //        httpRequest.Accept = "*/*";
    //        //Add custom headers e.g. JWT, CustomerUid, UserUid
    //        if (customHeaders != null)
    //        {
    //          foreach (var key in customHeaders.Keys)
    //          {
    //            if (key == "Content-Type")
    //            {
    //              httpRequest.ContentType = customHeaders[key];
    //            }
    //            else
    //            {
    //              httpRequest.Headers[key] = customHeaders[key];
    //            }
    //          }
    //        }
    //      }
    //      using (var writeStream = await request.GetRequestStreamAsync())
    //      {
    //        await payload.CopyToAsync(writeStream);
    //      }

    //      string responseString = null;
    //      using (var response = await request.GetResponseAsync())
    //      {
    //        responseString = GetStringFromResponseStream(response);
    //      }
    //      if (!string.IsNullOrEmpty(responseString))
    //        return JsonConvert.DeserializeObject<T>(responseString);
    //      return default(T);
    //    });

    //  if (policyResult.FinalException != null)
    //  {
    //    if (!suppressExceptionLogging)
    //    {
    //      log.LogDebug(
    //        "ExecuteRequest_multi(). exceptionToRethrow:{0} endpoint: {1} customHeaders: {2}",
    //        policyResult.FinalException.ToString(), endpoint, customHeaders);
    //    }
    //    throw policyResult.FinalException;
    //  }
    //  if (policyResult.Outcome == OutcomeType.Successful)
    //  {
    //    return policyResult.Result;
    //  }
    //  return default(T);
    //}


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
          writeStream.Write(bytes, 0, bytes.Length);
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
