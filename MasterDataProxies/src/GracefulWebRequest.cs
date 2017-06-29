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

      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);
          log.LogDebug("GracefulWebRequest.ExecuteRequest() : request{0}", JsonConvert.SerializeObject(request));

          string responseString = null;

          WebResponse response = null;
          try
          {
            response = await request.GetResponseAsync();
            if (response != null)
            {
              responseString = GetStringFromResponseStream(response);
              log.LogDebug("GracefulWebRequest.ExecuteRequest() : responseString{0}", responseString);
            }
          }
          catch (Exception ex)
          {
            if (response == null) throw;
            responseString = GetStringFromResponseStream(response);
            log.LogDebug("GracefulWebRequest.ExecuteRequestException() : responseString{0}", responseString);
            throw new Exception($"{ex.Message} {responseString}");
          }
          finally
          {
            response?.Dispose();
          }

          if (!string.IsNullOrEmpty(responseString))
          {
            var toReturn = JsonConvert.DeserializeObject<T>(responseString);
            log.LogDebug("GracefulWebRequest.ExecuteRequest(). toReturn:{0}",
              JsonConvert.SerializeObject(toReturn));
            return toReturn;
          }
          log.LogDebug("GracefulWebRequest.ExecuteRequest(). default(T):{0}",
            JsonConvert.SerializeObject(default(T)));
          var defaultToReturn = default(T);
          log.LogDebug("GracefulWebRequest.ExecuteRequest(). defaultToReturn:{0}",
            JsonConvert.SerializeObject(defaultToReturn));
          return defaultToReturn;
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            "GracefulWebRequest.ExecuteRequest(). exceptionToRethrow:{0} endpoint: {1} method: {2}, customHeaders: {3} payloadData: {4}",
            policyResult.FinalException.ToString(), endpoint, method, customHeaders, payloadData);
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
      var policyResult = await Policy
        .Handle<Exception>()
        .RetryAsync(retries)
        .ExecuteAndCaptureAsync(async () =>
        {
          var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

          Stream responseStream = null;
          using (var response = await request.GetResponseAsync())
          {
            responseStream = GetStreamFromResponse(response);
          }
          return responseStream;
        });

      if (policyResult.FinalException != null)
      {
        if (!suppressExceptionLogging)
        {
          log.LogDebug(
            "GracefulWebRequest.ExecuteRequest_stream(). exceptionToRethrow:{0} endpoint: {1} method: {2}, customHeaders: {3} payloadData: {4}",
            policyResult.FinalException.ToString(), endpoint, method, customHeaders, payloadData);
        }
        throw policyResult.FinalException;
      }
      if (policyResult.Outcome == OutcomeType.Successful)
      {
        return policyResult.Result;
      }
      return null;
    }

    private async Task<WebRequest> PrepareWebRequest(string endpoint, string method,
      IDictionary<string, string> customHeaders, string payloadData)
    {
      log.LogDebug("Requesting data from {0}", endpoint);
      if (customHeaders != null)
      {
        log.LogDebug("Custom Headers:");
        foreach (var key in customHeaders.Keys)
        {
          log.LogDebug("   {0}: {1}", key, customHeaders[key]);
        }
      }
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


    public async Task<T> ExecuteRequest<T>(string endpoint, Stream payload,
      IDictionary<string, string> customHeaders = null, int retries = 3, bool suppressExceptionLogging = false)
    {
      log.LogDebug("Requesting project data from {0}", endpoint);
      if (customHeaders != null)
      {
        log.LogDebug("Custom Headers:");
        foreach (var key in customHeaders.Keys)
        {
          log.LogDebug("   {0}: {1}", key, customHeaders[key]);
        }
      }

      //var request = WebRequest.Create(endpoint);
      //request.Method = "POST";
      //if (request is HttpWebRequest)
      //{
      //  var httpRequest = request as HttpWebRequest;
      //  httpRequest.Accept = "*/*";
      //  //Add custom headers e.g. JWT, CustomerUid, UserUid
      //  if (customHeaders != null)
      //  {
      //    foreach (var key in customHeaders.Keys)
      //    {
      //      httpRequest.Headers[key] = customHeaders[key];
      //    }
      //  }
      //}
      //using (var writeStream = await request.GetRequestStreamAsync())
      //{
      //  await payload.CopyToAsync(writeStream);
      //}

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
            "GracefulWebRequest.ExecuteRequest_multi(). exceptionToRethrow:{0} endpoint: {1} customHeaders: {2}",
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
