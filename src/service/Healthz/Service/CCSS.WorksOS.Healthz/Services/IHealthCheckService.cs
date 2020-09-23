using System.Collections.Generic;
using CCSS.WorksOS.Healthz.Models;
using CCSS.WorksOS.Healthz.Responses;

namespace CCSS.WorksOS.Healthz.Services
{
  public interface IHealthCheckService
  {
    /// <summary>
    /// Get a list of available services.
    /// </summary>
    IEnumerable<Service> GetServiceIdentifiers();

    /// <summary>
    /// Get the service state for n number of service identifiers.
    /// </summary>
    IEnumerable<ServicePingResponse> GetServiceState(params string[] serviceIdentifiers);
  }
}
