using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CCSS.IntegrationTests.Utils
{
  public class IntegrationTestRestClient : IIntegrationTestRestClient
  {
    private bool _disposed;
    private readonly ILogger _log;
    private readonly string _serviceBaseUrl;
    private readonly IHttpClientFactory _clientFactory;

    public IntegrationTestRestClient(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
      _log = loggerFactory.CreateLogger(GetType());
      _serviceBaseUrl = configuration.GetSection("ServiceBaseUrl").Value;
      _clientFactory = httpClientFactory;
    }

    public Task<HttpResponseMessage> SendAsync(string route, HttpMethod method, HttpHeaders customHeaders = null, string acceptHeader = MediaTypes.JSON, string contentType = MediaTypes.JSON, object body = null, string customerUid = null, string jwtToken = null)
    {
      var requestMessage = new HttpRequestMessage(method, new Uri(_serviceBaseUrl + route));

      if (body != null)
      {
        if (contentType == MediaTypes.JSON)
        {
          requestMessage.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, contentType);
        }
        else if (contentType.StartsWith(MediaTypes.MULTIPART_FORM_DATA))
        {
          requestMessage.Content = new ByteArrayContent(((MemoryStream)body).ToArray());
          requestMessage.Content.Headers.Add("Content-Type", contentType);
        }
      }

      requestMessage.Headers.Add("X-JWT-Assertion", string.IsNullOrEmpty(jwtToken)
                                   ? Auth.DEFAULT_JWT
                                   : jwtToken);

      requestMessage.Headers.Add("X-VisionLink-CustomerUid", customerUid);
      requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

      _log.LogInformation($"[{method}] {requestMessage.RequestUri.AbsoluteUri}");

      return _clientFactory.CreateClient().SendAsync(requestMessage);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
    {
      _log.LogInformation($"[{requestMessage.Method.Method}] {requestMessage.RequestUri.AbsoluteUri}");

      return _clientFactory.CreateClient().SendAsync(requestMessage);
    }
  }
}
