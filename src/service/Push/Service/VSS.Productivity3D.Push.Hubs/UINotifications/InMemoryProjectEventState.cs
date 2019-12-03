using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.UINotifications;

namespace VSS.Productivity3D.Push.Hubs.UINotifications
{
  /// <summary>
  /// In memory representation of the subscribed connection/projects- does not support scaling of the push service
  /// There can be multi projects allowed per connectionId
  /// </summary>
  public class InMemoryProjectEventState : IProjectEventState
  {
    private readonly List<KeyValuePair<string, ProjectEventSubscriptionModel>> connectionMapping = new List<KeyValuePair<string, ProjectEventSubscriptionModel>>();

    private readonly object lockObj = new object();
    
    /// <inheritdoc />
    public Task AddSubscription(string clientIdentifier, ProjectEventSubscriptionModel model)
    {
      lock(lockObj)
      {
        // Remove it it exists, note that a connection can subscribe to >1 project
        // (multiple browsers will have multiple connections)
        // connection may change customer, but new project subs for that customer will have diff customerUid AND projectUid
        // todoJeannie I don't get why you would need to remove this then add it.
        var exists = connectionMapping.Exists(k => k.Key == clientIdentifier && k.Value.ProjectUid == model.ProjectUid);
        if (exists)
          connectionMapping.Remove(connectionMapping.First(k => k.Key == clientIdentifier && k.Value.ProjectUid == model.ProjectUid));
        connectionMapping.Add(new KeyValuePair<string, ProjectEventSubscriptionModel>(clientIdentifier, model));
      }

      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveSubscription(string clientIdentifier)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task RemoveSubscriptions(string clientIdentifier)
    {
      lock (lockObj)
        connectionMapping.RemoveAll(item => item.Key.Equals(clientIdentifier));

      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<ProjectEventSubscriptionModel>> GetSubscriptions()
    {
      // can a user be logged on twice, looking at same customer/project?
      lock (lockObj)
        return Task.FromResult(connectionMapping.Select(s => s.Value).ToList());
    }

    /// <inheritdoc />
    public Task<List<string>> GetClientsForProject(Guid customerUid, Guid projectUid)
    {
      List<string> connections;
      lock (lockObj)
      {
        connections = connectionMapping
          .Where(k => k.Value.ProjectUid == projectUid && k.Value.CustomerUid == customerUid)
          .Select(k => k.Key)
          .ToList();
      }

      return Task.FromResult(connections);
    }
  }
}
