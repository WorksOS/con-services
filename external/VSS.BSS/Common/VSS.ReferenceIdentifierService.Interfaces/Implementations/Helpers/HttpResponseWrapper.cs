using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers
{
  public class HttpResponseWrapper : IHttpResponseWrapper
  {
    private readonly EasyHttp.Http.HttpResponse _httpResponse;

    public HttpResponseWrapper(EasyHttp.Http.HttpResponse httpResponse)
    {
      _httpResponse = httpResponse;
    }

    #region IHttpResponseWrapper Members

    public System.Net.HttpStatusCode StatusCode
    {
      get { return _httpResponse.StatusCode; }
    }

    public string RawText
    {
      get { return _httpResponse.RawText; }
    }

    public T StaticBody<T>(string overrideContentType = null)
    {
      return _httpResponse.StaticBody<T>(overrideContentType);
    }

    #endregion
  }
}
