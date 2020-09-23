using System.Collections.Generic;
using CCSS.WorksOS.Healthz.Models;
using CCSS.WorksOS.Healthz.Responses;
using CCSS.WorksOS.Healthz.Types;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace CCSS.WorksOS.Healthz.Services
{
  public interface IHealthCheckState
  {
    /// <summary>
    /// Adds a <see cref="ServiceResult"/> to the list of available servcies to poll for 'liveness'.
    /// </summary>
    Service AddPollingService(Service service);

    /// <summary>
    /// Returns a list of cache services used for polling.
    /// </summary>
    IEnumerable<Service> GetServiceIdentifiers();

    /// <summary>
    /// Add a new service state for the given service identifier.
    /// </summary>
    ServicePingResponse AddServicePingResponse(string identifier, ServicePingResponse servicePingResponse);

    /// <summary>
    /// Returns the last service state for n number of provided service identifiers.
    /// </summary>
    IEnumerable<ServicePingResponse> GetServiceState(params string[] identifiers);

    /// <summary>
    /// Sets the global service state; any one unavailable service will return a <see cref="ServiceState.Unavailable"/> response.
    /// </summary>
    void SetAggregatedServiceState(ServiceState serviceState);

    /// <summary>
    /// Returns a <see cref="ServiceState"/> response indicating all services are responding (Available), or any one of the polled services
    /// is non responsive (Unavailable).
    /// </summary>
    ServiceState GetAggregatedServiceState();

    // TODO Could include 'GetHistoryForServices(string[] identifiers)
  }
}
