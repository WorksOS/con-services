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
using TCCToDataOcean.Utils;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class RestClient : IRestClient
  {
    private const string BOUNDARY_START = "-----";

    private readonly HttpClient httpClient;
    private readonly ILogger Log;
    private readonly string _bearerToken;

    private static HttpRequestMessage GetRequestMessage(HttpMethod method, string uri) => new HttpRequestMessage(method, new Uri(uri));

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      Log = loggerFactory.CreateLogger<HttpClient>();
      Log.LogInformation(Method.In());

      _bearerToken = authentication.GetApplicationBearerToken();

      httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
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
      Log.LogInformation($"{Method.In()} URI: {method} {uri}");

      var request = GetRequestMessage(method, uri);

      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
      request.Headers.Add("X-VisionLink-CustomerUid", customerUid);
      request.Headers.Add("Authorization", $"Bearer {_bearerToken}");
      request.Headers.Add("X-JWT-Assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=");

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

      try
      {
        var response = await httpClient.SendAsync(request);

        var receiveStream = response.Content.ReadAsStreamAsync().Result;
        var readStream = new StreamReader(receiveStream, Encoding.UTF8);
        var responseBody = readStream.ReadToEnd();

        switch (response.StatusCode)
        {
          case HttpStatusCode.OK:
            {
              Log.LogInformation($"{Method.Info()} Status [{response.StatusCode}] Body: '{responseBody}'");
              break;
            }
          case HttpStatusCode.InternalServerError:
          case HttpStatusCode.NotFound:
            {
              Log.LogError($"{Method.Info()} Status [{response.StatusCode}] Body: '{responseBody}'");
              Debugger.Break();

              break;
            }
          default:
            {
              Log.LogDebug($"{Method.Info()} Status [{response.StatusCode}] URI: '{request.RequestUri.AbsoluteUri}', Body: '{responseBody}'");

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
        Log.LogError($"{Method.Info("ERROR")} {method} URI: '{request.RequestUri.AbsoluteUri}', Exception: {exception.GetBaseException()}");
      }

      return null;
    }
  }
}
