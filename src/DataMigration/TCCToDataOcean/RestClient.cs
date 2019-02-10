using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.Interfaces;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class RestClient : IRestClient
  {
    private readonly HttpClient httpClient;
    private readonly ILogger Log;

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      Log = loggerFactory.CreateLogger<HttpClient>();

      httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");

      var bearerToken = authentication.GetApplicationBearerToken();
      httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
    }

    public Task<HttpResponseMessage> SendHttpClientRequest(string uri, HttpMethod method, string payloadData, string acceptHeader, string contentType, string customerUid)
    {
      var request = new HttpRequestMessage(method, new Uri(uri));
      Log.LogInformation($"[{method}] {request.RequestUri.AbsoluteUri}");

      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
      request.Headers.Add("X-VisionLink-CustomerUid", customerUid);

      if (payloadData != null)
      {
        request.Content = new StringContent(payloadData, Encoding.UTF8, contentType);
      }

      return httpClient.SendAsync(request);
    }
  }
}
