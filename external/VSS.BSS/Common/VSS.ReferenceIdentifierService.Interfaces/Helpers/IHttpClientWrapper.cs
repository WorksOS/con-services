namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers
{
  public interface IHttpClientWrapper
  {
    IHttpResponseWrapper Get(string uri, object query = null);
    IHttpResponseWrapper Post(string uri, object query = null, object body = null);
    IHttpResponseWrapper Put(string uri, object query, object body);
  }
}
