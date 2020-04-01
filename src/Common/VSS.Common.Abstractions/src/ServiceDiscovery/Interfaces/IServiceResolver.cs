using System.Threading.Tasks;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace VSS.Common.Abstractions.ServiceDiscovery.Interfaces
{
  public interface IServiceResolver
  {
    /// <summary>
    /// Attempt to resolve an endpoint for a given service name
    /// </summary>
    /// <returns>An endpoint for the service, will be null if unknown</returns>
    Task<string> ResolveService(string serviceName);

    /// <summary>
    /// The service type this resolver is for
    /// </summary>
    ServiceResultType ServiceType { get; }

    /// <summary>
    /// Priority of the service resolution, where lower is high priority
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Is the Service Resolver Enabled? I.e does it have the resources needed to handle requests
    /// </summary>
    bool IsEnabled { get; }
  }
}
