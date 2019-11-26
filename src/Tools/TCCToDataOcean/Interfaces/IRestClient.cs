using System.Net.Http;
using System.Threading.Tasks;

namespace TCCToDataOcean.Interfaces
{
  public interface IRestClient
  {
    Task<TResponse> SendHttpClientRequest<TResponse>(
      string uri,
      HttpMethod method,
      string acceptHeader,
      string contentType,
      string customerUid = null,
      string requestBodyJson = null,
      byte[] payloadData = null,
      bool setJWTHeader = true) where TResponse : class;
  }
}
