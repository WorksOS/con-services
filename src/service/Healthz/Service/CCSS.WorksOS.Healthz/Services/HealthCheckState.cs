using System;
using System.Collections.Generic;
using System.Linq;
using CCSS.WorksOS.Healthz.Models;
using CCSS.WorksOS.Healthz.Responses;
using CCSS.WorksOS.Healthz.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace CCSS.WorksOS.Healthz.Services
{
  public interface IHealthCheckState
  {
    /// <summary>
    /// Adds a <see cref="ServiceResult"/> to the list of available servcies to poll for 'liveness'.
    /// </summary>
    Service AddPollingService(Service service);

    /// <summary>
    /// Returns a list of cache services used for polling.
    /// </summary>
    IEnumerable<Service> GetServiceIdentifiers();

    /// <summary>
    /// Add a new service state for the given service identifier.
    /// </summary>
    ServicePingResponse SetServiceState(string identifier, ServicePingResponse servicePingResponse);

    /// <summary>
    /// Returns the last service state for n number of provided service identifiers.
    /// </summary>
    IEnumerable<ServicePingResponse> GetServiceState(string[] identifiers);

    /// <summary>
    /// Sets the global service state; any one unavailable service will return a <see cref="ServiceState.Unavailable"/> response.
    /// </summary>
    void SetAggregatedServiceState(ServiceState serviceState);

    /// <summary>
    /// Returns a <see cref="ServiceState"/> response indicating all services are responding (Available), or any one of the polled services
    /// is non responsive (Unavailable).
    /// </summary>
    ServiceState GetAggregatedServiceState();

    // TODO Could include 'GetHistoryForServices(string[] identifiers)
  }

  public class HealthCheckState : IHealthCheckState
  {
    private const string SERVICE_IDENTIFIES_CACHE_KEY = "service-identifiers";
    private const string SERVICE_STATE_CACHE_KEY_PREFIX = "service-state-";

    private string _serviceStateCacheKey(string identifier) => SERVICE_STATE_CACHE_KEY_PREFIX + identifier;

    private readonly IMemoryCache _cache;
    private readonly ILogger _log;

    private ServiceState _aggregatedServiceState;

    public HealthCheckState(IMemoryCache memoryCache, ILoggerFactory loggerFactory)
    {
      _cache = memoryCache;
      _log = loggerFactory.CreateLogger<HealthCheckState>();

      _cache.Set(SERVICE_IDENTIFIES_CACHE_KEY, new Dictionary<string, Service>());
    }

    /// <inheritdoc/>
    public Service AddPollingService(Service service)
    {
      if (!_cache.TryGetValue(SERVICE_IDENTIFIES_CACHE_KEY, out Dictionary<string, Service> cachedIdentifiers))
      {
        throw new Exception($"Cache entry for service state not found at key '{SERVICE_IDENTIFIES_CACHE_KEY}'");
      }

      if (cachedIdentifiers.TryGetValue(service.Identifier, out var cachedService))
      {
        _log.LogInformation($"{nameof(AddPollingService)}: Found already cached service identifier '{service.Identifier}', updating entry.");

        cachedIdentifiers[service.Identifier] = service;
      }
      else
      {
        _log.LogInformation($"{nameof(AddPollingService)}: Setting cached service identifier '{service.Identifier}'");
        cachedIdentifiers.Add(service.Identifier, service);

        // Setup the cache entry for healthcheck ping records.
        var cacheKey = _serviceStateCacheKey(service.Identifier);
        _cache.Set(cacheKey, new List<ServicePingResponse>());
      }

      return service;
    }

    /// <inheritdoc/>
    public IEnumerable<Service> GetServiceIdentifiers()
    {
      if (!_cache.TryGetValue(SERVICE_IDENTIFIES_CACHE_KEY, out Dictionary<string, Service> cachedIdentifiers))
      {
        _log.LogInformation($"{nameof(GetServiceIdentifiers)}: Failed to find any cached service identifies under key '{SERVICE_IDENTIFIES_CACHE_KEY}'");
      }

      return cachedIdentifiers.Select(x => x.Value);
    }

    /// <inheritdoc/>
    public void SetAggregatedServiceState(ServiceState serviceState) => _aggregatedServiceState = serviceState;

    /// <inheritdoc/>
    public ServiceState GetAggregatedServiceState() => _aggregatedServiceState;

    /// <inheritdoc/>
    public ServicePingResponse SetServiceState(string identifier, ServicePingResponse servicePingResponse)
    {
      if (string.IsNullOrEmpty(identifier))
      {
        throw new ArgumentNullException(identifier, "Cannot be null");
      }

      var cacheKey = _serviceStateCacheKey(identifier);

      if (!_cache.TryGetValue(cacheKey, out List<ServicePingResponse> cachedResponses))
      {
        throw new Exception($"Cache entry for service '{cacheKey}' not found");
      }

      cachedResponses.Add(servicePingResponse);

      return servicePingResponse;
    }

    /// <inheritdoc/>
    public IEnumerable<ServicePingResponse> GetServiceState(string[] identifiers)
    {
      if (identifiers == null || identifiers.Length == 0)
      {
        throw new ArgumentNullException("Service identifier cannot be null or empty");
      }

      var response = new ServicePingResponse[identifiers.Length];

      for (var i = 0; i < identifiers.Length; i++)
      {
        var serviceCacheExists = _cache.TryGetValue(_serviceStateCacheKey(identifiers[i]), out List<ServicePingResponse> serviceState);

        if (!serviceCacheExists)
        {
          continue;
        }

        // Return the last, most recent ping response.
        response[i] = serviceState.Last();
      }

      return response;
    }
  }
}
