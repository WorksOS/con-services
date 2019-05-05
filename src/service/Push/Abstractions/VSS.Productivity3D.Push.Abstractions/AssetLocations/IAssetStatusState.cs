using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;

namespace VSS.Productivity3D.Push.Abstractions.AssetLocations
{
  /// <summary>
  /// Used to track Asset Status State, as hubs don't allow state to be persisted (they're transient)
  /// </summary>
  public interface IAssetStatusState
  {
    /// <summary>
    /// Add a subscription for a client
    /// </summary>
    Task AddSubscription(string clientIdentifier, AssetUpdateSubscriptionModel model);

    /// <summary>
    /// Remove a subscription
    /// </summary>
    Task RemoveSubscription(string clientIdentifier);

    /// <summary>
    /// Get all subscriptions currently active
    /// </summary>
    /// <returns></returns>
    Task<List<AssetUpdateSubscriptionModel>> GetSubscriptions();

    /// <summary>
    /// Get any clients that have requested statuses for a Customer / Project
    /// </summary>
    Task<List<string>> GetClientsForProject(Guid customerUid, Guid projectUid);
  }
}
