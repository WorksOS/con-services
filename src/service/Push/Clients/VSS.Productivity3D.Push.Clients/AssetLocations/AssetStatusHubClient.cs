using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;

namespace VSS.Productivity3D.Push.Clients.AssetLocations
{
  /// <summary>
  /// Client for the Asset Status Server (allowing for sending of Asset events, and getting Subscribed Asset information)
  /// </summary>
  public class AssetStatusServerHubClient : BaseClient, IAssetStatusServerHubClient
  {
    public AssetStatusServerHubClient(IConfigurationStore configuration, IServiceResolution resolver,
      ILoggerFactory loggerFactory)
      : base(configuration, resolver, loggerFactory)
    {
    }

    public override string HubRoute => HubRoutes.ASSET_STATUS_SERVER;

    public override void SetupCallbacks()
    {
      // No need for callbacks here, as this is called and generates no actions for anyone other than the actual hub
    }

    /// <summary>
    /// Get the list of subscriptions requested by the Asset Status Clients (UI)
    /// </summary>
    public async Task<List<AssetUpdateSubscriptionModel>> GetSubscriptions()
    {
      if (Connected)
        return await Connection.InvokeAsync<List<AssetUpdateSubscriptionModel>>(
          nameof(IAssetStatusServerHub.GetSubscriptions));

      Logger.LogWarning("Not connected - cannot get data, returning an empty list");
      return new List<AssetUpdateSubscriptionModel>();
    }

    /// <summary>
    /// Trigger an event for a particular Asset - this will cause the signalR hub to notify all relevant UI clients
    /// </summary>
    public async Task<bool> UpdateAssetLocationsForClient(AssetAggregateStatus assets)
    {
      if (Connected)
        return await Connection.InvokeAsync<bool>(nameof(IAssetStatusServerHub.UpdateAssetLocationsForClient), assets);

      Logger.LogWarning("Not connected - cannot update locations");
      return false;
    }
  }
}
