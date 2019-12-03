using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.UINotifications;

namespace VSS.Productivity3D.Push.Clients.UINotification
{
  /// <summary>
  /// Client for the Project Event Server (allowing for sending of Project events, and getting Subscribed Project events)
  /// </summary>
  public class ProjectEventHubClient : BaseClient, IProjectEventServerHubClient
  {
    public ProjectEventHubClient(IConfigurationStore configuration, IServiceResolution resolver,
      ILoggerFactory loggerFactory)
      : base(configuration, resolver, loggerFactory)
    {
    }

    public override string HubRoute => HubRoutes.PROJECT_EVENT_SERVER;

    public override void SetupCallbacks()
    {
      // No need for callbacks here, as this is called and generates no actions for anyone other than the actual hub
    }

    /// <summary>
    /// Get the list of subscriptions requested by the Project Event Clients (UI)
    /// </summary>
    public async Task<List<ProjectEventSubscriptionModel>> GetSubscriptions()
    {
      if (Connected)
        return await Connection.InvokeAsync<List<ProjectEventSubscriptionModel>>(
          nameof(IProjectEventServerHub.GetSubscriptions));

      Logger.LogWarning("Not connected - cannot get data, returning an empty list");
      return new List<ProjectEventSubscriptionModel>();
    }

    /// <summary>
    /// Trigger an event for a particular project - this will cause the signalR hub to notify all relevant UI clients
    /// </summary>
    public async Task<bool> AddProjectEventForClients(/* todoJeannie AssetAggregateStatus assets */)
    {
      if (Connected)
        return await Connection.InvokeAsync<bool>(nameof(IProjectEventServerHub.AddProjectEventForClients) /* todoJeannie , assets */);

      Logger.LogWarning("Not connected - cannot add project event");
      return false;
    }
  }
}
