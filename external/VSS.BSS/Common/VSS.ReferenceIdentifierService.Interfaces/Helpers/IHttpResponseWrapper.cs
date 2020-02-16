using System.Net;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers
{
  public interface IHttpResponseWrapper
  {
    HttpStatusCode StatusCode { get; }
    string RawText { get; }
    T StaticBody<T>(string overrideContentType = null);
  }
}