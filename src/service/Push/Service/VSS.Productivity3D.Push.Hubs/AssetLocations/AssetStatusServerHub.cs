using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;

namespace VSS.Productivity3D.Push.Hubs.AssetLocations
{
  /// <summary>
  /// This hub allows other internal services to trigger asset location updates for all clients that have requested events
  /// </summary>
  public class AssetStatusServerHub : AuthenticatedHub<IAssetStatusServerHub>, IAssetStatusServerHub
  {
    private readonly IAssetStatusState assetState;
    private readonly IHubContext<AssetStatusClientHub, IAssetStatusClientHubContext> hub;

    public AssetStatusServerHub(ILoggerFactory loggerFactory, IAssetStatusState assetState, IHubContext<AssetStatusClientHub, IAssetStatusClientHubContext> hub) : base(loggerFactory)
    {
      this.assetState = assetState;
      this.hub = hub;
    }

    /// <inheritdoc />
    public async Task<List<AssetUpdateSubscriptionModel>> GetSubscriptions()
    {
      return await assetState.GetSubscriptions();
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAssetLocationsForClient(AssetAggregateStatus assets)
    {
      var connections = await assetState.GetClientsForProject(assets.CustomerUid, assets.ProjectUid);
      await hub.Clients.Clients(connections).UpdateAssetStatus(assets);
      return true;
    }
  }
}
