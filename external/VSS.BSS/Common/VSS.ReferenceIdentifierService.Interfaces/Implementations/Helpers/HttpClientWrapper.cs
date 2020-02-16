using EasyHttp.Http;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers
{
  public class HttpClientWrapper : IHttpClientWrapper
  {
    private readonly HttpClient _httpClient;

    public HttpClientWrapper(HttpClient httpClient = null)
    {
      _httpClient = httpClient;
    }

    #region IHttpClientWrapper Members

    public IHttpResponseWrapper Get(string uri, object query)
    {
      return new HttpResponseWrapper(_httpClient.Get(uri, query));
    }

    public IHttpResponseWrapper Post(string uri, object query, object body)
    {
      return new HttpResponseWrapper(_httpClient.Post(uri, body, HttpContentTypes.TextPlain, query));
    }

    public IHttpResponseWrapper Put(string uri, object query, object body)
    {
      return new HttpResponseWrapper(_httpClient.Put(uri, body, HttpContentTypes.TextPlain, query));
    }

    #endregion
  }
}
