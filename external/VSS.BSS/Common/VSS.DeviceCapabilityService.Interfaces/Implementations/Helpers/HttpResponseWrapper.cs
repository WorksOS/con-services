using System.Collections.ObjectModel;
using System.Net;
using EasyHttp.Codecs;
using EasyHttp.Http;
using JsonFx.Serialization;
using JsonFx.Serialization.Providers;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
    public class HttpResponseWrapper : IHttpResponseWrapper
    {
      private readonly HttpResponse _httpResponse;

      public HttpStatusCode StatusCode
      {
        get { return _httpResponse.StatusCode; }
      }

      public string RawText
      {
        get { return _httpResponse.RawText; }
      }

      public HttpResponseWrapper(HttpResponse httpResponse = null)
      {
        _httpResponse = httpResponse ??
                        new HttpResponse(new DefaultDecoder(new DataReaderProvider(new Collection<IDataReader>())));
      }

      public T StaticBody<T>(string overrideContentType = null)
      {
        return _httpResponse.StaticBody<T>(overrideContentType);
      }
    }
}
