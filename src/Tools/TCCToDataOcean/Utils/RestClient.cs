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
    private readonly string _jwtToken;

    private static HttpRequestMessage GetRequestMessage(HttpMethod method, string uri) => new HttpRequestMessage(method, new Uri(uri));

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication, IEnvironmentHelper environmentHelper)
    {
      _log = loggerFactory.CreateLogger<HttpClient>();
      _log.LogInformation(Method.In());

      _bearerToken = authentication.GetApplicationBearerToken();
      _jwtToken = environmentHelper.GetVariable("JWT_TOKEN", 1);

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
      string customerUid = null,
      string requestBodyJson = null,
      byte[] payloadData = null,
      bool setJWTHeader = true) where TResponse : class
    {
      _log.LogInformation($"{Method.In()} URI: {method} {uri}");

      var request = GetRequestMessage(method, uri);

      try
      {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

        if (!string.IsNullOrEmpty(customerUid)) { request.Headers.Add("X-VisionLink-CustomerUid", customerUid); }

        request.Headers.Add("Authorization", $"Bearer {_bearerToken}");

        if (setJWTHeader) { request.Headers.Add("X-JWT-Assertion", _jwtToken); }

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
          case HttpStatusCode.Unauthorized:
            {
              Debugger.Break();

              break;
            }
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
