using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Responses;
using CCSS.WorksOS.Healthz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;

namespace CCSS.WorksOS.Healthz.Controllers
{
  [Route("api/[controller]")]
  public class HealthCheckController : BaseController<HealthCheckController>
  {
    private readonly IMemoryCache _cache;
    private readonly IServiceResolution _serviceResolution;
    private static List<string> _serviceIdentifiers;

    static HealthCheckController()
    {
      _serviceIdentifiers = ServiceResolver.GetKnownServiceIdentifiers();
    }

    public HealthCheckController(IMemoryCache memoryCache, IServiceResolution serviceResolution)
    {
      _cache = memoryCache;
      _serviceResolution = serviceResolution;
    }

    [HttpGet("v1/serviceidentifiers")]
    public IActionResult GetServiceIdentifiers()
    {
      return Ok(new
      {
        Services = _serviceIdentifiers
      });
    }

    [HttpGet("v1/service")]
    public async Task<IActionResult> GetServiceStatusSingle(string serviceIdentifier)
    {
      var sw = Stopwatch.StartNew();

      var pingResponse = await _serviceResolution
        .ResolveLocalServiceEndpoint(serviceIdentifier, ApiType.Private, ApiVersion.V1, "/ping", null)
        .ContinueWith(x => ServicePingResponse.Create(serviceIdentifier, sw.Elapsed, x.IsFaulted));

      return Ok(new[] { pingResponse });
    }

    [HttpGet("v1/services")]
    public async Task<IActionResult> GetServiceStatusAll()
    {
      var tasks = new List<Task>(_serviceIdentifiers.Count);
      var serviceRespones = new List<ServicePingResponse>(tasks.Count);

      foreach (var i in _serviceIdentifiers)
      {
        var sw = Stopwatch.StartNew();

        tasks.Add(_serviceResolution.ResolveLocalServiceEndpoint(i, ApiType.Private, ApiVersion.V1, "/ping", null)
           .ContinueWith(x => serviceRespones.Add(ServicePingResponse.Create(i, sw.Elapsed, false))));
      }

      await Task.WhenAll(tasks);

      return Ok(new[] { serviceRespones });
    }
  }
}
