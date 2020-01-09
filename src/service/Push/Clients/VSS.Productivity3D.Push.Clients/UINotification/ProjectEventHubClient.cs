using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.UINotifications;

namespace VSS.Productivity3D.Push.Clients.UINotification
{
  /// <summary>
  /// Client for the Project Event Server (allowing for sending of Project events).
  /// </summary>
  public class ProjectEventHubClient : BaseClient, IProjectEventHubClient
  {
   
    public ProjectEventHubClient(IConfigurationStore configuration,
      IServiceResolution resolution,
      ILoggerFactory loggerFactory)
      : base(configuration, resolution, loggerFactory)
    {
     }

    /// <inheritdoc />
    public override string HubRoute => HubRoutes.PROJECT_EVENTS;

    public override void SetupCallbacks()
    {
      // No need for callbacks here, as this is called and generates no actions for anyone other than the actual hub
    }

    // todoJeannie may not be needed in the hubClient?      
    public Task SubscribeToProjectEvents(Guid projectUid)
    {
      if (Connected)
        return Connection.InvokeAsync(nameof(IProjectEventHub.StartProcessingProject), projectUid);

      // We could queue this up if it becomes a problem
      Logger.LogWarning("Attempt to subscribe to project while client disconnected. Subscription failed.");
      return Task.CompletedTask;
    }

    public Task FileImportIsComplete(ImportedFileStatus importedFileStatus)
    {
      if (Connected)
        return Connection.InvokeAsync(nameof(IProjectEventHub.SendImportedFileEventToClients), importedFileStatus);

      // We could queue this up if it becomes a problem
      Logger.LogWarning("Attempt to send completion message while client disconnected. Completion not sent.");
      return Task.CompletedTask;
    }
  }
}
