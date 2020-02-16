using EasyHttp.Http;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
      private readonly HttpClient _httpClient;

      public HttpClientWrapper(HttpClient httpClient = null)
      {
        _httpClient = httpClient;
      }

      public IHttpResponseWrapper Get(string uri, object query = null)
      {
        return new HttpResponseWrapper(_httpClient.Get(uri, query));
      }
    }
}
