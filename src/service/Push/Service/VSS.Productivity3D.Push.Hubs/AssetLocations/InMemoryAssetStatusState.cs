using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;

namespace VSS.Productivity3D.Push.Hubs.AssetLocations
{
  /// <summary>
  /// In memory representation of the assets subscribed - does not support scaling of the push service
  /// </summary>
  public class InMemoryAssetStatusState : IAssetStatusState
  {
    private readonly Dictionary<string, AssetUpdateSubscriptionModel> connectionMapping = new Dictionary<string, AssetUpdateSubscriptionModel>();

    private readonly object lockObj = new object();
    
    /// <inheritdoc />
    public Task AddSubscription(string clientIdentifier, AssetUpdateSubscriptionModel model)
    {
      lock(lockObj)
      {
        // Remove it it exists, a connection can only subscribe to one project
        // (multiple browsers will have multiple connections)
        connectionMapping.Remove(clientIdentifier);
        connectionMapping.Add(clientIdentifier, model);
      }

      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveSubscription(string clientIdentifier)
    {
      lock (lockObj)
        connectionMapping.Remove(clientIdentifier);

      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<AssetUpdateSubscriptionModel>> GetSubscriptions()
    {
      lock (lockObj)
      {
        return Task.FromResult(connectionMapping.Values.ToList());
      }
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
