using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// Used to track subscriptions, as hubs don't allow state to be persisted (they're transient)
  /// </summary>
  public interface IProjectEventState
  {
    /// <summary>
    /// Add a subscription for a client
    /// </summary>
    Task AddSubscription(string clientIdentifier, ProjectEventSubscriptionModel model);

    /// <summary>
    /// Remove a subscription
    /// </summary>
    Task RemoveSubscription(string clientIdentifier);

    /// <summary>
    /// Remove all subscriptions under this client
    ///   Used for ProjectEvents which can have >1 per client
    /// </summary>
    Task RemoveSubscriptions(string clientIdentifier);

    /// <summary>
    /// Get all subscriptions currently active
    /// </summary>
    /// <returns></returns>
    Task<List<ProjectEventSubscriptionModel>> GetSubscriptions();

    /// <summary>
    /// Get any clients that have requested statuses for a Customer / Project
    /// </summary>
    Task<List<string>> GetClientsForProject(Guid customerUid, Guid projectUid);
  }
}
