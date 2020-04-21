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
  public class RestClient : IRestClient, IDisposable
  {
    private bool disposed;
    private readonly ILogger log;
    private readonly string serviceBaseUrl;
    private readonly HttpClient httpClient;

    public RestClient(ILoggerFactory loggerFactory, IConfiguration configuration, HttpClient httpClient = null)
    {
      log = loggerFactory.CreateLogger(GetType());
      serviceBaseUrl = configuration.GetSection("ServiceBaseUrl").Value;

      this.httpClient = httpClient ?? new HttpClient();

      this.httpClient.DefaultRequestHeaders.Add("X-VisionLink-ClearCache", "true");
      this.httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
    }

    public Task<HttpResponseMessage> SendAsync(string route, HttpMethod method, HttpHeaders customHeaders = null, string acceptHeader = MediaTypes.JSON, string contentType = MediaTypes.JSON, object body = null, string customerUid = null, string jwtToken = null)
    {
      var requestMessage = new HttpRequestMessage(method, new Uri(serviceBaseUrl + route));

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

      log.LogInformation($"[{method}] {requestMessage.RequestUri.AbsoluteUri}");

      return httpClient.SendAsync(requestMessage);
    }

    ~RestClient()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposed)
      {
        return;
      }

      if (disposing)
      {
        httpClient?.Dispose();
      }

      disposed = true;
    }
  }
}
