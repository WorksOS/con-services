using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace VSS.MasterData.Proxies
{
  internal static class HttpClientExtensions
  {
    public static Task<HttpResponseMessage> GetAsync
      (this HttpClient httpClient, string uri, int? timeout, Action<HttpRequestMessage> preAction, ILogger log)
    {
      var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

      preAction(httpRequestMessage);

      var cancellationTime = timeout.HasValue ? timeout.Value : 60000;
      var cts = new CancellationTokenSource(cancellationTime);
      log?.LogDebug($"Starting the request with timeout: {cancellationTime}ms");
      return httpClient.SendAsync(httpRequestMessage,cts.Token);
    }

    public static Task<HttpResponseMessage> PostAsync
    (this HttpClient httpClient, string uri, Stream requestStream, HttpMethod method,
      IDictionary<string, string> customHeaders, int? timeout, Action<HttpRequestMessage> preAction, ILogger log)
    {
      //Default to JSON content type
      HttpContent content = null;
      if (requestStream != null)
      {
        const string DefaultContentType = "application/json";
       
        var contentType = MediaTypeHeaderValue.Parse(DefaultContentType);
        if (customHeaders != null && customHeaders.ContainsKey(HeaderNames.ContentType))
        {
          if (MediaTypeHeaderValue.TryParse(customHeaders[HeaderNames.ContentType], out var ct))
            contentType = ct;
          else
            log.LogWarning($"Failed to convert {customHeaders[HeaderNames.ContentType]} to a valid Content-Type.");
        }
        // The content coming in with already be encoded correctly (for strings, via Encoding.UTF8.GetBytes())
        content = new StreamContent(requestStream);
        content.Headers.ContentType = contentType;
        
      }

      //Default to POST, nothing else is supported so far apart from PUT and GET
      var httpRequestMessage = new HttpRequestMessage(method, uri)
      {
        Content = content
      };
      preAction(httpRequestMessage);
      var cancellationTime = timeout.HasValue ? timeout.Value : 60000;
      var cts = new CancellationTokenSource(cancellationTime);
      log?.LogDebug($"Starting the request with timeout: {cancellationTime}ms");
      return httpClient.SendAsync(httpRequestMessage,cts.Token);
    }
  }
}