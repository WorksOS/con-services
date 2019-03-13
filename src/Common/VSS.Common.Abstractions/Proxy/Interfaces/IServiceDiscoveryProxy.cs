using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace VSS.Common.Abstractions.Proxy.Interfaces
{
  public interface IServiceDiscoveryProxy
  {
    /// <summary>
    /// Is this Service Local to us?
    /// If the service is near this service in terms of network layout and doesn't need to go via TPaaS or other
    /// When the Service is Inside our Authentication Boundary then we will pass extra authentication information that would normally be added by the Authentication layer if it were an external request
    /// </summary>
    bool IsInsideAuthBoundary { get; }

    /// <summary>
    /// The service this proxy is for, if we are accessing an internal service
    /// </summary>
    ApiService InternalServiceType { get; }

    /// <summary>
    /// If we are not accessing an internal service, this variable will be used for the service discovery
    /// </summary>
    string ExternalServiceName { get; }

    /// <summary>
    /// The version of the API this proxy is for
    /// </summary>
    ApiVersion Version { get; }

    /// <summary>
    /// The Type of API this service is for, public means it is exposed via TPaaS, so the URL includes /api/version/endpoint
    /// </summary>
    ApiType Type { get; }

    /// <summary>
    /// If we have a specific cache key for the expiry for cached items
    /// </summary>
    string CacheLifeKey { get; }
  }
}