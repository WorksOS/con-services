using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.WorksOS.Healthz.Services
{
  public class HealthCheckService : IHealthCheckService
  {
    private readonly ILogger _log;
    private readonly IWebRequest _webRequest;

    public HealthCheckService(ILoggerFactory loggerFactory, IWebRequest webRequest)
    {
      _log = loggerFactory.CreateLogger<HealthCheckService>();
      _webRequest = webRequest;
    }

    private bool IsSuccessStatusCode(HttpStatusCode statusCode) => (int)statusCode >= 200 && (int)statusCode <= 299;

    public async Task<ServicePingResponse> QueryService(string serviceIdentifier, string url, IHeaderDictionary customHeaders)
    {
      var pingUrl = url.TrimEnd('/') + "/ping";

      try
      {
        _log.LogInformation($"{nameof(QueryService)}: Querying '{pingUrl}'...");
        var sw = Stopwatch.StartNew();

        var response = await _webRequest.ExecuteRequest(
          endpoint: pingUrl,
          customHeaders: customHeaders,
          method: HttpMethod.Get,
          retries: 0);

        sw.Stop();

        _log.LogInformation($"{nameof(QueryService)}: Service responded in {sw.Elapsed}");
        return ServicePingResponse.Create(serviceIdentifier, sw.ElapsedTicks, IsSuccessStatusCode(response));
      }
      catch (Exception ex)
      {
        _log.LogError($"{nameof(QueryService)}: Failure querying service '{serviceIdentifier}' at '{pingUrl}'; {ex.GetBaseException().Message}");
        return null;
      }
    }
  }
}
