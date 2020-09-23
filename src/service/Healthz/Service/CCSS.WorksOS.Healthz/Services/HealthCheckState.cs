using System;
using System.Collections.Generic;
using System.Linq;
using CCSS.WorksOS.Healthz.Models;
using CCSS.WorksOS.Healthz.Responses;
using CCSS.WorksOS.Healthz.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CCSS.WorksOS.Healthz.Services
{
  public class HealthCheckState : IHealthCheckState
  {
    private const string SERVICE_IDENTIFIES_CACHE_KEY = "service-identifiers";
    private const string SERVICE_STATE_CACHE_KEY_PREFIX = "service-state-";

    private const int PING_RESPONSE_CACHE_SIZE = 5;

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
        _cache.Set(cacheKey, new Queue<ServicePingResponse>(PING_RESPONSE_CACHE_SIZE));
      }

      return service;
    }

    /// <inheritdoc/>
    public IEnumerable<Service> GetServiceIdentifiers()
    {
      if (_cache.TryGetValue(SERVICE_IDENTIFIES_CACHE_KEY, out Dictionary<string, Service> cachedIdentifiers))
      {
        return cachedIdentifiers.Select(x => x.Value);
      }

      _log.LogInformation($"{nameof(GetServiceIdentifiers)}: Failed to find any cached service identifies under key '{SERVICE_IDENTIFIES_CACHE_KEY}'");
      return null;
    }

    /// <inheritdoc/>
    public void SetAggregatedServiceState(ServiceState serviceState) => _aggregatedServiceState = serviceState;

    /// <inheritdoc/>
    public ServiceState GetAggregatedServiceState() => _aggregatedServiceState;

    /// <inheritdoc/>
    public ServicePingResponse AddServicePingResponse(string identifier, ServicePingResponse servicePingResponse)
    {
      if (string.IsNullOrEmpty(identifier))
      {
        throw new ArgumentNullException(identifier, "Cannot be null");
      }

      var cacheKey = _serviceStateCacheKey(identifier);

      if (!_cache.TryGetValue(cacheKey, out Queue<ServicePingResponse> cachedResponses))
      {
        throw new Exception($"Cache entry for service '{cacheKey}' not found");
      }

      cachedResponses.Enqueue(servicePingResponse);

      if (cachedResponses.Count > PING_RESPONSE_CACHE_SIZE)
      {
        cachedResponses.TrimExcess();
      }

      return servicePingResponse;
    }

    /// <inheritdoc/>
    public IEnumerable<ServicePingResponse> GetServiceState(params string[] identifiers)
    {
      if (identifiers == null || identifiers.Length == 0)
      {
        throw new ArgumentNullException("Service identifier cannot be null or empty");
      }

      var response = new ServicePingResponse[identifiers.Length];

      for (var i = 0; i < identifiers.Length; i++)
      {
        var serviceCacheExists = _cache.TryGetValue(_serviceStateCacheKey(identifiers[i]), out Queue<ServicePingResponse> serviceStates);

        if (!serviceCacheExists || serviceStates.Count == 0)
        {
          continue;
        }

        // Return the last, most recent ping response.
        response[i] = serviceStates.Last();
      }

      return response;
    }
  }
}
