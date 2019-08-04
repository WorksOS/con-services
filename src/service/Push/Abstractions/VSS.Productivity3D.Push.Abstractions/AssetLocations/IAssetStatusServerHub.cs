using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;

namespace VSS.Productivity3D.Push.Abstractions.AssetLocations
{
  /// <summary>
  /// Server side hub for triggering asset location events (not publicly accessible) 
  /// </summary>
  public interface IAssetStatusServerHub
  {
    /// <summary>
    /// Get the list of subscriptions requested by the Asset Status Clients (UI)
    /// </summary>
    Task<List<AssetUpdateSubscriptionModel>> GetSubscriptions();

    /// <summary>
    /// Trigger an event for a particular Asset - notifying all clients who are interested in this asset
    /// </summary>
    Task<bool> UpdateAssetLocationsForClient(AssetAggregateStatus assets);
  }
}
