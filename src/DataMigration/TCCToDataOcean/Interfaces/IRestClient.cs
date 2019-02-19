using System.Net.Http;
using System.Threading.Tasks;

namespace TCCToDataOcean.Interfaces
{
  public interface IRestClient
  {
    Task<HttpResponseMessage> SendHttpClientRequest(string uri, HttpMethod method, string payloadData, string acceptHeader, string contentType, string customerUid);
    //Task<HttpResponseMessage> SendHttpClientRequest(string uri, HttpMethod method, byte[] payloadData, string acceptHeader, string contentType, string customerUid);
  }
}
