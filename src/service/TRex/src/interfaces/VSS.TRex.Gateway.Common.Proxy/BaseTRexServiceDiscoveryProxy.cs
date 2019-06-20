using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Proxy.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;

namespace VSS.TRex.Gateway.Common.Proxy
{
  /// <summary>
  /// Service Discovery enabled Proxy
  /// For now, we inherit from the BaseProxy to get code related to caching
  /// But we should create brand new fetch methods than don't accept URL values
  /// As these should be 'resolved' by the Service Resolution class
  /// </summary>
  public abstract class BaseTRexServiceDiscoveryProxy : BaseServiceDiscoveryProxy
  {
    private readonly IServiceResolution _serviceResolution;

    protected BaseTRexServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
      _serviceResolution = serviceResolution;
    }

    #region Properties

    /// <summary>
    /// The Type of gateway this service is for, so the service-name includes it e.g. trex-gateway; trex-mutable-gateway; trex-connectedSite-gateway
    /// </summary>
    protected GatewayType Gateway { get; set; } = GatewayType.None;

    #endregion

    #region Protected Methods

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>
    protected override Task<string> GetUrl(string route = null, IDictionary<string, string> queryParameters = null)
    {
      return _serviceResolution.ResolveLocalServiceEndpoint(GetServiceName(), Type, Version, route, queryParameters);
    }

    private string GetServiceName()
    {
      switch (Gateway)
      {
        case GatewayType.Immutable:
          return ServiceNameConstants.TREX_SERVICE_IMMUTABLE;
        case GatewayType.Mutable:
          return ServiceNameConstants.TREX_SERVICE_MUTABLE;
        case GatewayType.ConnectedSite:
          return ServiceNameConstants.TREX_SERVICE_CONNECTEDSITE;
        default:
          throw new ArgumentOutOfRangeException("Trex", Gateway, null);
      }
    }

    #endregion

  }
}
