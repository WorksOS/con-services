using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Exceptions;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace VSS.Common.Abstractions.ServiceDiscovery
{
  public class InternalServiceResolver : IServiceResolution
  {
    private const int CACHE_SERVICE_ENDPOINT_TIMEOUT_SECONDS = 10;

    private const string URL_ROUTE_PATH_SEPARATOR = "/";

    private const string URL_FIRST_PARAMETER_SEPARATOR = "?";
    private const string URL_OTHER_PARAMETER_SEPARATOR  = "&";

    private readonly ILogger<InternalServiceResolver> logger;
    private readonly IDataCache cache;

    public InternalServiceResolver(IEnumerable<IServiceResolver> serviceResolvers, ILogger<InternalServiceResolver> logger, IDataCache cache)
    {
      this.logger = logger;
      this.cache = cache;
      Resolvers = serviceResolvers.OrderBy(s => s.Priority).ToList();
      logger.LogInformation($"We have {Resolvers.Count} Service Resolvers:");
      logger.LogInformation("-----");
      foreach (var serviceResolver in Resolvers)
      {
        logger.LogInformation($"\t{serviceResolver.GetType().Name}");
        logger.LogInformation($"\t\tPriority: {serviceResolver.Priority}");
        logger.LogInformation($"\t\tService Type: {serviceResolver.ServiceType}");
        logger.LogInformation($"\t\tEnabled: {serviceResolver.IsEnabled}");
      }
      logger.LogInformation("-----");
    }

    /// <inheritdoc />
    public List<IServiceResolver> Resolvers { get; }

    /// <inheritdoc />
    public async Task<ServiceResult> ResolveService(string serviceName)
    {
      foreach (var serviceResolver in Resolvers)
      {
        if (!serviceResolver.IsEnabled)
          continue;
        try
        {
          var endPoint = await serviceResolver.ResolveService(serviceName);
          if (!string.IsNullOrEmpty(endPoint))
          {
            return new ServiceResult
            {
              Endpoint = endPoint,
              Type = serviceResolver.ServiceType
            };
          }
        }
        catch (Exception e)
        {
          // We don't know what exceptions the resolve may throw
          logger.LogWarning(e, $"Failed to resolve service '{serviceName}' due to error");
        }
      }

      return new ServiceResult
      {
        Type = ServiceResultType.Unknown,
        Endpoint = null
      };
    }

    /// <summary>
    /// Coverts an Productivity 3D (not to be confused with 3dp API) API Service Enum to a service name,
    /// to minimize the chance of errors when typing service names
    ///
    /// TRex uses its own resolver in BaseTRexServiceDiscoveryProxy
    /// </summary>
    /// <returns>The service name string for the API Service Enum Value</returns>
    public string GetServiceName(ApiService service)
    {
      switch (service)
      {
        case ApiService.Project:
          return Constants.ServiceNameConstants.PROJECT_SERVICE;
        case ApiService.Filter:
          return Constants.ServiceNameConstants.FILTER_SERVICE;
        case ApiService.Productivity3D:
          return Constants.ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
        case ApiService.Scheduler:
          return Constants.ServiceNameConstants.SCHEDULER_SERVICE;
        case ApiService.AssetMgmt3D:
          return Constants.ServiceNameConstants.ASSETMGMT3D_SERVICE;
        case ApiService.Push:
          return Constants.ServiceNameConstants.PUSH_SERVICE;
        case ApiService.Tile:
          return Constants.ServiceNameConstants.TILE_SERVICE;
        case ApiService.TagFileAuth:
          return Constants.ServiceNameConstants.TAGFIELAUTH_SERVICE;

        default:
          // There are unit tests to ensure this does not happen 
          throw new ArgumentOutOfRangeException(nameof(service), service, null);
      }
    }

    /// <summary>
    /// Generates the service name for an explicit Type and Version to be searched for.
    /// For example, if you wanted to configure Scheduler Public V2 API to be different, you could set a specific config value to override the default
    /// </summary>
    /// <returns>Service Name value for a specific API Version and Type</returns>
    public string GetServiceConfigurationName(string serviceName, ApiType apiType, ApiVersion version) => $"{serviceName}_{apiType.GetDescription()}_{version.GetDescription()}".ToLower();

    /// <summary>
    /// This is a naming convention for our APIS
    /// /api/v1/endpoint is for publicly exposed APIs via TPaaS Or other
    /// /internal/v1/endpoint is for private APIs only accessed by out services
    /// </summary>
    /// <returns>A URL component representing the API Type</returns>
    public string GetApiRoute(ApiType apiType)
    {
      switch (apiType)
      {
        case  ApiType.Private:
          return "internal";
        case ApiType.Public:
          return "api";
        default:
          throw new ArgumentOutOfRangeException(nameof(apiType), apiType, null);
      }
    }

    /// <inheritdoc />
    public Task<string> ResolveLocalServiceEndpoint(ApiService service, ApiType apiType, ApiVersion version, string route = null,
      IList<KeyValuePair<string, string>> queryParameters = null)
    {
      var serviceName = GetServiceName(service);
      return ResolveRemoteServiceEndpoint(serviceName, apiType, version, route, queryParameters);
    }

    public Task<string> ResolveLocalServiceEndpoint(string serviceName, ApiType apiType, ApiVersion version, string route = null,
      IList<KeyValuePair<string, string>> queryParameters = null)
    {
      return ResolveRemoteServiceEndpoint(serviceName, apiType, version, route, queryParameters);
    }

    /// <inheritdoc />
    public async Task<string> ResolveRemoteServiceEndpoint(string serviceName, ApiType apiType, ApiVersion version,
      string route = null,
      IList<KeyValuePair<string, string>> queryParameters = null)
    {
      // Cache the service endpoint for a given service name
      var cacheKey = $"{nameof(InternalServiceResolver)}-{GetServiceConfigurationName(serviceName, apiType, version)}";
      var url = await cache.GetOrCreate(cacheKey, async entry =>
      {
        entry.SetOptions(new MemoryCacheEntryOptions()
        {
          AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_SERVICE_ENDPOINT_TIMEOUT_SECONDS)
        });

        var result = await GetUrl(serviceName, apiType, version);
        var cacheItem = new CacheItem<string>(result, new List<string>
        {
          serviceName
        });

        return cacheItem;
      });

      logger.LogInformation($"Request for Service {serviceName}, Type: {apiType}, Version: {version} result: {url}");

      if (string.IsNullOrEmpty(url))
        throw new ServiceNotFoundException(serviceName);

      // Now we have a URL, attempt to add the routes
      if (string.IsNullOrEmpty(route))
      {
        if(queryParameters != null)
          throw new ArgumentException($"Query Parameters passed in with no URL Route");
        return url;
      }

      var sb = new StringBuilder(url);
        
      if (route.StartsWith(URL_ROUTE_PATH_SEPARATOR))
        sb.Append(route);
      else
        sb.Append(URL_ROUTE_PATH_SEPARATOR + route);

      if (queryParameters == null) 
        return sb.ToString();

      var first = true;

      foreach (var parameter in queryParameters)
      {
        sb.Append(first ? URL_FIRST_PARAMETER_SEPARATOR : URL_OTHER_PARAMETER_SEPARATOR);
        sb.Append($"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}");
        first = false;
      }
      
      return sb.ToString();
    }

    private async Task<string> GetUrl(string serviceName, ApiType apiType, ApiVersion version)
    {
      string url;
      
      // We will see if we have an explicit service defined for this version of the service
      // If not we will attempt to build it from the base url
      var service = await ResolveService(GetServiceConfigurationName(serviceName, apiType, version));
      if (service != null && service.Type != ServiceResultType.Unknown)
      {
        url = service.Endpoint.TrimEnd(URL_ROUTE_PATH_SEPARATOR.ToCharArray());
      }
      else
      {
        service = await ResolveService(serviceName);
        if (service == null || service.Type == ServiceResultType.Unknown)
          return null;

        var apiComponent = GetApiRoute(apiType);
        var versionComponent = version.GetDescription().ToLower();
        var endpoint = service.Endpoint.TrimEnd(URL_ROUTE_PATH_SEPARATOR.ToCharArray());

        url = $"{endpoint}{URL_ROUTE_PATH_SEPARATOR}{apiComponent}{URL_ROUTE_PATH_SEPARATOR}{versionComponent}";
      }

      return url;
    }
  }
}
