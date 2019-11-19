using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using VSS.WebApi.Common;

namespace TCCToDataOcean.Utils
{
  public class RestClient : IRestClient
  {
    private const string BOUNDARY_START = "-----";

    private readonly HttpClient _httpClient;
    private readonly ILogger _log;
    private readonly string _bearerToken;

    private static HttpRequestMessage GetRequestMessage(HttpMethod method, string uri) => new HttpRequestMessage(method, new Uri(uri));

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      _log = loggerFactory.CreateLogger<HttpClient>();
      _log.LogInformation(Method.In());

      _bearerToken = authentication.GetApplicationBearerToken();

      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
    }

    /// <summary>
    /// Multi purpose HttpClient request wrapper.
    /// </summary>
    public async Task<TResponse> SendHttpClientRequest<TResponse>(
      string uri,
      HttpMethod method,
      string acceptHeader,
      string contentType,
      string customerUid,
      string requestBodyJson = null,
      byte[] payloadData = null) where TResponse : class
    {
      _log.LogInformation($"{Method.In()} URI: {method} {uri}");

      var request = GetRequestMessage(method, uri);

      try
      {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
        request.Headers.Add("X-VisionLink-CustomerUid", customerUid);
        request.Headers.Add("Authorization", $"Bearer {_bearerToken}");
        request.Headers.Add("X-JWT-Assertion", "eyJ4NXQiOiJNREk0WTJNd1pUVmpNRGRsTkdVeFlUSmtaV00zWmpCaVptVTBZVFprTkRjek9HTmhZMk5oTmciLCJraWQiOiJNREk0WTJNd1pUVmpNRGRsTkdVeFlUSmtaV00zWmpCaVptVTBZVFprTkRjek9HTmhZMk5oTmciLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnRMb2NrZWQiOiJmYWxzZSIsInN1YiI6ImYzMWZiYWYwLTEzMGMtNDMzYi1iOWEwLTdhZDc1YjgzZjQyYSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmFpbGVkTG9naW5BdHRlbXB0cyI6IjAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IlVubGltaXRlZCIsImlzcyI6Imh0dHBzOi8vaWRlbnRpdHkudHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNTI2NDYzMjEwMDkyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOjEsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTU0MDc1NDgyNTA5OSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6ImYzMWZiYWYwLTEzMGMtNDMzYi1iOWEwLTdhZDc1YjgzZjQyYSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXNlckNyZWF0ZVRpbWVTdGFtcCI6IjE1MjY0NjMyMTAwOTIiLCJhenAiOiI5WDd6MkZKNXRBUVNNd05KVXhPaGZCcVVWTmdhIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiIzRCBGaWxlIG1pZ3JhdGlvbiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJkZXYiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2ZpcnN0bmFtZSI6InZzcyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXNlcnR5cGUiOiJBUFBMSUNBVElPTiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6MCwiZXhwIjoxNTY5NTUzODgwLCJpYXQiOjE1Njk1NDk5NDEsImh0dHA6Ly90cmltYmxlLmNvbS90cGFhcy9jbGFpbXMvYXBwbGljYXRpb25uYW1lIjoiUHJvZC1FYXJ0aHdvcmtzLUVDNTIwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLTNkcHJvZHVjdGl2aXR5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9wYXNzd29yZFRpbWVzdGFtcCI6IjE1MjY0NjMyMTAwOTIiLCJodHRwOi8vdHJpbWJsZS5jb20vdHBhYXMvY2xhaW1zL3VzZXJuYW1lIjoiY3RjdC10cGFhcy11Z0B0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjIuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlByb2QtRWFydGh3b3Jrcy1FQzUyMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlciI6ImN0Y3QtdHBhYXMtdWdAdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RVcGRhdGVUaW1lU3RhbXAiOiIxNTY2ODU2MjY4NTM2IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJjdGN0IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbFZlcmlmaWVkIjoidHJ1ZSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudHVzZXJuYW1lIjoiY3RjdC10cGFhcy11ZyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly90cmltYmxlLmNvbS90cGFhcy9jbGFpbXMvYXBwbGljYXRpb25pZCI6ImE1Yjg4OWU5LTg4N2MtNDQ1Yi05OWE5LTBkNDc2NmU4MTAyNSIsImh0dHA6Ly90cmltYmxlLmNvbS90cGFhcy9jbGFpbXMvY29uc3VtZXJrZXkiOiI5WDd6MkZKNXRBUVNNd05KVXhPaGZCcVVWTmdhIn0.Ausn-rt6KJsihUoYfeh2Hh70OjNHdfB6lKwoDRQRApI");

        if (requestBodyJson != null || payloadData != null)
        {
          switch (acceptHeader)
          {
            case MediaType.APPLICATION_JSON:
              {
                request.Content = new StringContent(requestBodyJson, Encoding.UTF8, contentType);
                break;
              }
            case MediaType.MULTIPART_FORM_DATA:
              {
                contentType = $"multipart/form-data; boundary={BOUNDARY_START}{Guid.NewGuid().ToString()}";
                throw new NotImplementedException();
              }
            default:
              {
                throw new Exception($"Unsupported content type '{contentType}'");
              }
          }
        }

        var response = await _httpClient.SendAsync(request);

        var receiveStream = response.Content.ReadAsStreamAsync().Result;
        string responseBody;

        using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
        {
          responseBody = readStream.ReadToEnd();
        }

        switch (response.StatusCode)
        {
          case HttpStatusCode.OK:
            {
              _log.LogInformation($"{Method.Info()} Status [{response.StatusCode}] Body: '{responseBody}'");
              break;
            }
          case HttpStatusCode.InternalServerError:
          case HttpStatusCode.NotFound:
            {
              _log.LogError($"{Method.Info()} Status [{response.StatusCode}] Body: '{responseBody}'");
              Debugger.Break();

              break;
            }
          default:
            {
              _log.LogDebug($"{Method.Info()} Status [{response.StatusCode}] URI: '{request.RequestUri.AbsoluteUri}', Body: '{responseBody}'");

              if (response.StatusCode == HttpStatusCode.Unauthorized)
              {
                Debugger.Break();
              }
              break;
            }
        }

        switch (response.Content.Headers.ContentType.MediaType)
        {
          case MediaType.APPLICATION_JSON:
            {
              return JsonConvert.DeserializeObject<TResponse>(responseBody);
            }
          case MediaType.TEXT_PLAIN:
          case MediaType.APPLICATION_OCTET_STREAM:
            {
              return await response.Content.ReadAsStringAsync() as TResponse;
            }
          default:
            {
              throw new Exception($"Unsupported content type '{response.Content.Headers.ContentType.MediaType}'");
            }
        }
      }
      catch (Exception exception)
      {
        _log.LogError($"{Method.Info("ERROR")} {method} URI: '{request.RequestUri.AbsoluteUri}', Exception: {exception.GetBaseException()}");
      }
      finally
      {
        request.Dispose();
      }

      return null;
    }
  }
}
