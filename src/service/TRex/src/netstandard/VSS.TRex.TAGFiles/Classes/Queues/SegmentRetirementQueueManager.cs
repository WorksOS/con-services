using System;
using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  /// <summary>
  /// Responsible for management of querying the TAG file buffer queue for work to do.
  /// Utilizes Ignite continuous queries and needs to be instantiated in context, unlike the grid deployed service model
  /// </summary>
  public class SegmentRetirementQueueManager : IDisposable
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SegmentRetirementQueueManager>();

    /// <summary>
    /// Local Ignite resource reference
    /// </summary>
    private readonly IIgnite _ignite;

    /// <summary>
    /// No-arg constructor. Instantiates the continuous query and performs initial scan of elements that the remote filter 
    /// will populate into the node-local groupers within the mutable grid.
    /// </summary>
    public SegmentRetirementQueueManager(bool runLocally)
    {
      _log.LogInformation("Establishing segment retirement queue cache context");

      // Get the ignite grid and cache references

      _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());
      var queueCache = _ignite.GetCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(TRexCaches.SegmentRetirementQueueCacheName());

      // Todo: Create a thread to periodically (needed if we don't go down the service route
      // ....

      _log.LogInformation("Completed segment retirement queue manager initialization");
    }

    public void Dispose()
    {
      _ignite?.Dispose();
    }
  }
}
