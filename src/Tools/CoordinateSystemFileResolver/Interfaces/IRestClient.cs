using System.Net.Http;

namespace CoordinateSystemFileResolver.Interfaces
{
  public interface IRestClient
  {
    TResponse SendHttpClientRequest<TResponse>(
      string uri,
      HttpMethod method,
      string acceptHeader,
      string contentType,
      string customerUid = null,
      string requestBodyJson = null,
      byte[] payloadData = null) where TResponse : class;
  }
}
