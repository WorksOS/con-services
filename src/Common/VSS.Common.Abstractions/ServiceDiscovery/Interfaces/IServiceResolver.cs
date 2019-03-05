using System.Threading.Tasks;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

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
  }
}
