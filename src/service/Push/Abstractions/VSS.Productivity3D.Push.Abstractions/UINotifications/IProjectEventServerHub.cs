using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// Server side hub for triggering project events (not publicly accessible) 
  /// </summary>
  public interface IProjectEventServerHub
  {
    /// <summary>
    /// Get the list of subscriptions requested by the project Event Clients (UI)
    /// </summary>
    Task<List<ProjectEventSubscriptionModel>> GetSubscriptions();

    /// <summary>
    /// Trigger an event for a particular project - notifying all clients who are interested in this project
    /// </summary>
    Task<bool> AddProjectEventForClients(/* todoJeannie AssetAggregateStatus assets */);
  }
}
