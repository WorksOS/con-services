using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IWebRequest
  {
    Task<HttpContent> ExecuteRequestAsStreamContent(string endpoint, HttpMethod method,
      IHeaderDictionary customHeaders = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);

    Task<T> ExecuteRequest<T>(string endpoint, Stream payload = null,
      IHeaderDictionary customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);

    Task ExecuteRequest(string endpoint, Stream payload = null,
      IHeaderDictionary customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);
  }
}
