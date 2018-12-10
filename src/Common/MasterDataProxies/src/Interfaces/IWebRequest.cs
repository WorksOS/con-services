using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IWebRequest
  {
    Task<HttpContent> ExecuteRequestAsStreamContent(string endpoint, HttpMethod method,
      IDictionary<string, string> customHeaders = null, Stream payloadStream = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);

    Task<T> ExecuteRequest<T>(string endpoint, Stream payload = null,
      IDictionary<string, string> customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);

    Task ExecuteRequest(string endpoint, Stream payload = null,
      IDictionary<string, string> customHeaders = null, HttpMethod method = null,
      int? timeout = null, int retries = 3, bool suppressExceptionLogging = false);
  }
}
