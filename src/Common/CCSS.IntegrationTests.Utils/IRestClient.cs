using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils.Types;

namespace CCSS.IntegrationTests.Utils
{
  public interface IRestClient
  {
    Task<HttpResponseMessage> SendHttpClientRequest(string route, HttpMethod method, HttpHeaders customHeaders = null, string acceptHeader = MediaTypes.JSON, string contentType = MediaTypes.JSON, object body = null, string customerUid = null, string jwtToken = null);
  }
}
