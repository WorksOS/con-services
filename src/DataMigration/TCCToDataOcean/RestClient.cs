using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Utils;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class MediaTypes
  {
    public const string JSON = "application/json";
    public const string MULTIPART_FORM_DATA = "multipart/form-data";
  }

  public class RestClient : IRestClient
  {
    private readonly HttpClient httpClient;
    private readonly ILogger Log;

    private static HttpRequestMessage GetRequestMessage(HttpMethod method, string uri) => new HttpRequestMessage(method, new Uri(uri));

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      Log = loggerFactory.CreateLogger<HttpClient>();
      Log.LogInformation(Method.In);

      var bearerToken = authentication.GetApplicationBearerToken();

      httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
      httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

      Log.LogInformation(Method.Out);
    }

    public Task<HttpResponseMessage> SendHttpClientRequest(string uri, HttpMethod method, string payloadData, string acceptHeader, string contentType, string customerUid)
    {
      Log.LogInformation(Method.In);

      var request = GetRequestMessage(method, uri);

      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
      request.Headers.Add("X-VisionLink-CustomerUid", customerUid);

      if (payloadData != null)
      {
        switch (contentType)
        {
          case MediaTypes.JSON:
            {
              request.Content = new StringContent(payloadData, Encoding.UTF8, contentType);

              break;
            }
          case MediaTypes.MULTIPART_FORM_DATA:
            {
              throw new NotImplementedException();

              contentType = $"{MediaTypes.MULTIPART_FORM_DATA}; boundary=-----{Guid.NewGuid().ToString()}";
            }
        }
      }

      Log.LogInformation(Method.Out);

      return httpClient.SendAsync(request);
    }
  }
}
