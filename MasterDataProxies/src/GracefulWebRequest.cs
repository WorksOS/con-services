using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<T> ExecuteRequest<T>(string endpoint, string method,
            IDictionary<string, string> customHeaders = null, string payloadData = null)
        {
            var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);
            log.LogDebug("GracefulWebRequest.ExecuteRequest() : request{0}", JsonConvert.SerializeObject(request));

            return await Policy
                .Handle<Exception>() // ( e => log.LogError("GracefulWebRequest.RetryOnException(){0}", e.ToString()))
                .Retry(3, (exception, retryCount) => {log.LogError("GracefulWebRequest.RetryOnException(){0}", exception.ToString());})
                .ExecuteAndCapture(async () =>
                {
                  try
                  {
                    string responseString = null;
                    using (var response = await request.GetResponseAsync())
                    {
                      log.LogDebug("GracefulWebRequest.ExecuteRequest(). response{0}",
                        JsonConvert.SerializeObject(response));
                      responseString = GetStringFromResponseStream(response);
                      log.LogDebug("GracefulWebRequest.ExecuteRequest() : responseString{0}", responseString);
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
                  }
                  catch (Exception e)
                  {
                    log.LogDebug("GracefulWebRequest.ExecuteRequest(). exception:{0}", e.ToString());
                  }
                  return default(T);
                })
                .Result;
        }

        public async Task<Stream> ExecuteRequest(string endpoint, string method,
            IDictionary<string, string> customHeaders = null, string payloadData = null)
        {
            var request = await PrepareWebRequest(endpoint, method, customHeaders, payloadData);

            return await Policy
                .Handle<Exception>()
                .Retry(3)
                .ExecuteAndCapture(async () =>
                {
                    using (var response = await request.GetResponseAsync())
                    {
                         return GetStreamFromResponse(response);
                    }
                })
                .Result;
        }

        private async Task<WebRequest> PrepareWebRequest(string endpoint, string method, IDictionary<string, string> customHeaders, string payloadData)
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
                        httpRequest.Headers[key] = customHeaders[key];
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
                    var reader = new StreamReader(readStream, Encoding.UTF8);
                    var responseString = reader.ReadToEnd();
                    log.LogDebug("Response: {0}", responseString);
                    return responseString;
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
            IDictionary<string, string> customHeaders = null)
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
                        httpRequest.Headers[key] = customHeaders[key];
                    }
                }
            }
            using (var writeStream = await request.GetRequestStreamAsync())
            {
              await payload.CopyToAsync(writeStream);
            }
            return await Policy
                .Handle<Exception>()
                .Retry(3)
                .ExecuteAndCapture(async () =>
                {
                    string responseString = null;
                    using (var response = await request.GetResponseAsync())
                    {
                        responseString = GetStringFromResponseStream(response);
                    }
                    if (!string.IsNullOrEmpty(responseString))
                        return JsonConvert.DeserializeObject<T>(responseString);
                    return default(T);
                }).Result;
        }

    }
}
