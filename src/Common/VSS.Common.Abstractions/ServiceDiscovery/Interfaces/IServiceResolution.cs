using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Exceptions;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace VSS.Common.Abstractions.ServiceDiscovery.Interfaces
{
  public interface IServiceResolution
  {
    /// <summary>
    /// The list of resolvers available to resolve a service endpoint
    /// </summary>
    List<IServiceResolver> Resolvers { get; }

    /// <summary>
    /// Resolves a service endpoint, working from the highest priority to the lowest
    /// </summary>
    /// <returns>Unknown result if not found, otherwise a service result outlining the endpoints</returns>
    Task<ServiceResult> ResolveService(string serviceName);

    /// <summary>
    /// Convert an internal service definition to a string representation
    /// </summary>
    /// <exception cref="ArgumentException">The Service requested is not defined. </exception>
    string GetServiceName(ApiService service);

    /// <summary>
    /// Resolve a Service URL for a given Service that we know about (e.g project / filter)
    /// This will handle the cases where the service is internal / via TPaaS
    /// </summary>
    /// <exception cref="ServiceNotFoundException"></exception>
    /// <returns>Full URL Representing the service details, or an exception if no service found</returns>
    Task<string> ResolveLocalServiceEndpoint(ApiService service, ApiType apiType, ApiVersion version, string route = null, IList<KeyValuePair<string, string>> queryParameters = null);

    /// <summary>
    /// Resolve a Service URL for a given Service that we know about (e.g project / filter)
    /// This will handle the cases where the service is internal / via TPaaS
    /// </summary>
    /// <exception cref="ServiceNotFoundException"></exception>
    /// <returns>Full URL Representing the service details, or an exception if no service found</returns>
    Task<string> ResolveLocalServiceEndpoint(string service, ApiType apiType, ApiVersion version, string route = null, IList<KeyValuePair<string, string>> queryParameters = null);

    /// <summary>
    /// Resolve a Service URL for a given Service Name (either our service, or an external service that is configured)
    /// This will handle the cases where the service is internal / via TPaaS
    /// </summary>
    /// <exception cref="ServiceNotFoundException"></exception>
    /// <returns>Full URL Representing the service details, or an exception if no service found</returns>
    Task<string> ResolveRemoteServiceEndpoint(string serviceName, ApiType apiType, ApiVersion version, string route = null, IList<KeyValuePair<string, string>> queryParameters = null);
  }
}
