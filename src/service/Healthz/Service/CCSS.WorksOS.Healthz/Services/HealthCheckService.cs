using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Responses;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.WorksOS.Healthz.Services
{
  public class HealthCheckService : IHealthCheckService
  {
    private readonly IWebRequest _webRequest;

    public HealthCheckService(IWebRequest webRequest)
    {
      _webRequest = webRequest;
    }

    private bool IsSuccessStatusCode(HttpStatusCode statusCode) => (int)statusCode >= 200 && (int)statusCode <= 299;

    public async Task<ServicePingResponse> QueryService(string serviceIdentifier, string url, IHeaderDictionary customHeaders)
    {
      var pingUrl = url.TrimEnd('/') + "/ping";

      var sw = Stopwatch.StartNew();

      var response = await _webRequest.ExecuteRequest(
        endpoint: pingUrl,
        customHeaders: customHeaders,
        method: HttpMethod.Get,
        retries: 0);

      return ServicePingResponse.Create(serviceIdentifier, sw.ElapsedTicks, IsSuccessStatusCode(response));
    }
  }
}
