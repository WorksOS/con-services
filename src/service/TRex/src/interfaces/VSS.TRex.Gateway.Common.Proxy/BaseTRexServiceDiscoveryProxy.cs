using System;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Proxy.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
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
  public abstract class BaseTRexServiceDiscoveryProxy : BaseServiceDiscoveryProxy, IServiceDiscoveryProxy
  {

    protected BaseTRexServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore,
      ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    #region Properties

    /// <summary>
    /// The Type of gateway this service is for, so the service-name includes it e.g. trex-gateway; trex-mutable-gateway; trex-connectedSite-gateway
    /// </summary>
    public abstract GatewayType Gateway { get; set; }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Execute a Post to an endpoint, and cache the result
    /// NOTE: Must have a uid or userid for cache key
    /// </summary>

    public string GetServiceName(ApiService service, GatewayType gateway)
    {
      switch (service)
      {
        case ApiService.TRex:
          switch (gateway)
          {
            case GatewayType.Mutable:
              return ServiceNameConstants.TREX_MUTABLE_SERVICE;
            case GatewayType.ConnectedSite:
              return ServiceNameConstants.TREX_CONNECTEDSITE_SERVICE;
            default:
              return ServiceNameConstants.TREX_SERVICE;
          }
        default:
          // There are unit tests to ensure this does not happen 
          throw new ArgumentOutOfRangeException(nameof(service), service, null);
      }
    }

    #endregion

  }
}
