using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.Tpaas.Client.Constants;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Generic RESTful httpClient request/response handler.
  /// </summary>
  public class CoordinateServiceHttpClient
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CoordinatesServiceClient>();
    private readonly HttpClient _httpClient;

    public CoordinateServiceHttpClient(HttpClient httpClient)
    {
      this._httpClient = httpClient;
      httpClient.DefaultRequestHeaders.Add(HeaderConstants.ACCEPT, MediaTypes.JSON);
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
      var absoluteUriPath = _httpClient.BaseAddress.AbsoluteUri + route;
      
      _log.LogInformation($"{nameof(CoordinateServiceHttpClient)}: {nameof(SendRequest)}: ({HttpMethod.Put.Method})|{absoluteUriPath}");

      return _httpClient.PutAsync(absoluteUriPath, content);
    }

    private Task<HttpResponseMessage> SendHttpClientRequest(string route, HttpMethod method, HttpContent httpContent = null)
    {
      var absoluteUriPath = _httpClient.BaseAddress.AbsoluteUri + route;

      _log.LogInformation($"{nameof(CoordinateServiceHttpClient)}: {nameof(SendRequest)}: ({method})|{absoluteUriPath}");

      var requestMessage = new HttpRequestMessage(method, absoluteUriPath)
      {
        Content = httpContent
      };

      return _httpClient.SendAsync(requestMessage);
    }
  }
}
