using System.Collections.Generic;
using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Services;
using CCSS.WorksOS.Healthz.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace CCSS.WorksOS.Healthz.Controllers
{
  public class HealthCheckController : BaseController<HealthCheckController>
  {
    private readonly IMemoryCache _cache;
    private readonly IHealthCheckService _healthCheckService;

    private readonly IServiceResolution _serviceResolution;
    private static List<string> _serviceIdentifiers;
    private static Dictionary<string, ServiceResult> _services;

    static HealthCheckController()
    {
      _serviceIdentifiers = ServiceResolver.GetKnownServiceIdentifiers();
    }

    public HealthCheckController(IMemoryCache memoryCache, IServiceResolution serviceResolution, IHealthCheckService healthCheckService)
    {
      _cache = memoryCache;
      _serviceResolution = serviceResolution;
      _healthCheckService = healthCheckService;
    }

    [HttpGet("api/v1/service")]
    public IActionResult GetServiceIdentifiers()
    {
      return Ok(new
      {
        Services = _serviceIdentifiers
      });
    }

    [HttpGet("api/v1/service/{name}/status")]
    public async Task<IActionResult> GetServiceStatusSingle(string name)
    {
      // TODO validate inputs.

      // TODO This will be moved to the HealthCheck hosted service and this method will instead return the last known state 
      // held by the hosted service's cache.
      var serviceResult = await _serviceResolution.ResolveService(serviceName: name);

      var result = await _healthCheckService.QueryService(name, serviceResult.Endpoint, CustomHeaders);

      return Ok(result);
    }

    [HttpGet("api/v1/service/status")]
    public async Task<IActionResult> GetServiceStatusAll()
    {
      if (_services == null)
      {
        // TODO This will be moved to the HealthCheck hosted service.
        var serviceResultTasks = new List<Task>(_serviceIdentifiers.Count);
        _services = new Dictionary<string, ServiceResult>();

        foreach (var identifier in _serviceIdentifiers)
        {
          serviceResultTasks
            .Add(_serviceResolution.ResolveService(serviceName: identifier)
            .ContinueWith(x =>
            {
              var serviceResult = x.Result;

              if (string.IsNullOrEmpty(serviceResult.Endpoint) || serviceResult.Endpoint.Contains("localhost"))
              {
                return;
              }

              if (!_services.TryGetValue(identifier, out var _))
              {
                _services.Add(identifier, serviceResult);
              }
            }));
        }

        await Task.WhenAll(serviceResultTasks);
      }

      var tasks = new List<Task>(_services.Count);

      var aggregatedServiceState = ServiceState.Unknown;

      foreach (var i in _services)
      {
        tasks
          .Add(_healthCheckService.QueryService(i.Key, i.Value.Endpoint, CustomHeaders)
          .ContinueWith(x =>
          {
            if (x.Result.State != ServiceState.Available)
            {
              aggregatedServiceState = ServiceState.Unavailable;
            }

            if (aggregatedServiceState != ServiceState.Unavailable)
            {
              aggregatedServiceState = ServiceState.Available;
            }
          }));
      }

      await Task.WhenAll(tasks);

      return Ok(new { servicesOnline = aggregatedServiceState == ServiceState.Available });
    }
  }
}
