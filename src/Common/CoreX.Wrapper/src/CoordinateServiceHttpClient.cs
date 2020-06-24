using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSS.Tpaas.Client.Constants;

namespace CoreX.Wrapper
{
  /// <summary>
  /// Generic RESTful httpClient request/response handler.
  /// </summary>
  public class CoordinateServiceHttpClient
  {
    private readonly HttpClient httpClient;

    public CoordinateServiceHttpClient(HttpClient httpClient)
    {
      this.httpClient = httpClient;
      httpClient.DefaultRequestHeaders.Add("Accept", MediaTypes.JSON);
    }

    /// <summary>
    /// Sends a standard request that might include query parameters, most likely a GET request.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, HttpMethod method) =>
      SendHttpClientRequest(route, method);

    /// <summary>
    /// Sends a request with a JSON body content, such formdata POST.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, HttpMethod method, string contentType, string bodyContent) =>
      SendHttpClientRequest(route, method, new StringContent(bodyContent, Encoding.UTF8, contentType));

    /// <summary>
    /// Sends a PUT request with a binary attachment.
    /// </summary>
    public Task<HttpResponseMessage> SendRequest(string route, MultipartFormDataContent content) =>
      httpClient.PutAsync(httpClient.BaseAddress.AbsoluteUri + route, content);

    private Task<HttpResponseMessage> SendHttpClientRequest(string route, HttpMethod method, HttpContent httpContent = null)
    {
      var absoluteUriPath = httpClient.BaseAddress.AbsoluteUri + route;

      var requestMessage = new HttpRequestMessage(method, absoluteUriPath)
      {
        Content = httpContent
      };

      return httpClient.SendAsync(requestMessage);
    }
  }
}
