using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// <returns>Null if not found, otherwise a service result outlining the endpoints</returns>
    Task<ServiceResult> ResolveService(string serviceName);
  }
}