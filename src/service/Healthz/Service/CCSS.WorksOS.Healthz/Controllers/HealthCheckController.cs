using System.Linq;
using CCSS.WorksOS.Healthz.Services;
using CCSS.WorksOS.Healthz.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Healthz.Controllers
{
  public class HealthCheckController : BaseController<HealthCheckController>
  {
    private readonly IHealthCheckState _healthCheckState;
    private readonly IMemoryCache _cache;

    public HealthCheckController(IHealthCheckState healthCheckState, IMemoryCache memoryCache)
    {
      _healthCheckState = healthCheckState;
      _cache = memoryCache;
    }

    [HttpGet("api/v1/service")]
    public IActionResult GetServiceIdentifiers()
    {
      var services = _healthCheckState.GetServiceIdentifiers();

      return Ok(new
      {
        Services = services.Select(x => x.Identifier)
      });
    }

    [HttpGet("api/v1/service/{name}/status")]
    public IActionResult GetServiceStatusSingle(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        return BadRequest($"'{nameof(name)}' cannot be null");
      }

      Log.LogInformation($"{nameof(GetServiceStatusSingle)}: Resolving service status for '{name}'...");

      var result = _healthCheckState.GetServiceState(new[] { name });

      Log.LogInformation($"{nameof(GetServiceStatusSingle)}: '{name}' status is {JsonConvert.SerializeObject(result)}");

      return Ok(result);
    }

    [HttpGet("api/v1/service/status")]
    public IActionResult GetServiceStatusAll()
    {
      var result = _healthCheckState.GetAggregatedServiceState();

      Log.LogInformation($"{nameof(GetServiceStatusAll)}: Resolved aggregated service status, found: {result}");

      return Ok(new { servicesOnline = result == ServiceState.Available });
    }
  }
}
