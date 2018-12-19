using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.HttpClients.Constants;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Generic RESTful httpClient request/response handler.
  /// </summary>
  public class CoordinateServiceHttpClient
  {
    private static readonly ILogger log = Logging.Logger.CreateLogger<CoordinatesServiceClient>();
    private readonly HttpClient httpClient;

    public CoordinateServiceHttpClient(HttpClient httpClient)
    {
      this.httpClient = httpClient;
    }

    /// <summary>
    /// Sends a standard request that might include query parameters, most likely a GET request.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, HttpMethod method)
    {
      return SendHttpClientRequest(route, method);
    }

    /// <summary>
    /// Sends a request with a JSON body content, such formdata POST.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, HttpMethod method, string contentType, string bodyContent)
    {
      return SendHttpClientRequest(route, method, new StringContent(bodyContent, Encoding.UTF8, contentType));
    }

    /// <summary>
    /// Sends a PUT request with a binary attachment.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, MultipartFormDataContent content)
    {
      var absoluteUriPath = httpClient.BaseAddress.AbsoluteUri + route;
      
      log.LogInformation($"{nameof(CoordinateServiceHttpClient)}: {nameof(SendRequest)}: ({HttpMethod.Put.Method})|{absoluteUriPath}");

      return httpClient.PutAsync(absoluteUriPath, content);
    }

    private Task<HttpResponseMessage> SendHttpClientRequest(string route, HttpMethod method, HttpContent httpContent = null)
    {
      var absoluteUriPath = httpClient.BaseAddress.AbsoluteUri + route;

      log.LogInformation($"{nameof(CoordinateServiceHttpClient)}: {nameof(SendRequest)}: ({method})|{absoluteUriPath}");

      var requestMessage = new HttpRequestMessage(method, absoluteUriPath)
      {
        Content = httpContent
      };

      httpClient.DefaultRequestHeaders.Add("Accept", MediaTypes.JSON);

      return httpClient.SendAsync(requestMessage);
    }
  }
}
