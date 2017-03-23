using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace VSS.Raptor.Service.Common.Proxies
{
  public class GracefulWebRequest
  {
    private readonly ILogger log;

    public GracefulWebRequest(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GracefulWebRequest>();
    }

    public async Task<T> ExecuteRequest<T>(string endpoint, string method, IDictionary<string, string> customHeaders = null, string payloadData = null)
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
      //////////////////////////////////////////////////////
      // TEMPORARY CODE until .netcore 1.2 available with FileWebRequest 
      // then the real code should work for both HttpWebRequest and FileWebRequest
      /////////////////////////////////////////////////////
      if (!endpoint.ToLower().StartsWith("http")) //i.e. file
      {
        return GetFromFile<T>(endpoint);
      }
      //////// END TEMP CODE ///////

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
            httpRequest.Headers[key] = customHeaders[key];
          }
        }
        //TODO Add timeout here
        //httpRequest.Timeout = 10000;//not in .netcore
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

      return await Policy
        .Handle<Exception>()
        .Retry(3)
        .ExecuteAndCapture(async () =>
        {
          using (var response = await request.GetResponseAsync())
          {
              if (response.ContentLength > 0)
                  return JsonConvert.DeserializeObject<T>(GetStringFromResponseStream(response));
              return default(T);
          }
        }).Result;
    }

    private string GetStringFromResponseStream(WebResponse response)
    {
      using (var readStream = response.GetResponseStream())
      {

        if (readStream != null)
        {
          var reader = new StreamReader(readStream, Encoding.UTF8);
          var responseString = reader.ReadToEnd();
          log.LogDebug("Response: {0}", responseString);
          return responseString;
        }
        return string.Empty;
      }
    }


    //////// TEMP CODE ///////
    private T GetFromFile<T>(string endpoint)
    {
      // absorb any exception, returning null
      T result = default(T);
      log.LogDebug("GracefulRequest.GetFromFile(): Going to try to open file for reading: {0}. current directory:{1}", endpoint, System.IO.Directory.GetCurrentDirectory());

      try
      {
        using (FileStream fs = new FileStream(endpoint, FileMode.Open, FileAccess.Read))
        {
          if (fs != null)
          {
            using (StreamReader r = new StreamReader(fs))
            {
              string json = r.ReadToEnd();
              result = JsonConvert.DeserializeObject<T>(json);
            }
          }
          else
          {
            log.LogError("GracefulRequest.GetFromFile(): Failed to open file for reading: {0}. current directory:{1}", endpoint, System.IO.Directory.GetCurrentDirectory());
          }
        }
      }
      catch (Exception ex)
      {
        log.LogError("GracefulRequest.GetFromFile(): Exception getting data from file: {0}", ex);
      }

      return result;
    }
    //////// END TEMP CODE ///////
  }
}
