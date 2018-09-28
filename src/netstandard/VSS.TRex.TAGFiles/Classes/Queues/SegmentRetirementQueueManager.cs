using System;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  /// <summary>
  /// Responsible for management of querying the TAG file buffer queue for work to do.
  /// Utilises Ignite continuous queries and needs to be instantiated in context, unlike the grid deployed service model
  /// </summary>
  public class SegmentRetirementQueueManager : IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Local Ignite resource reference
    /// </summary>
    private IIgnite ignite;

    /// <summary>
    /// No-arg constructor. Instantiates the continuous query and performs initial scan of elements that the remote filter 
    /// will populate into the node-local groupers within the mutable grid.
    /// </summary>
    public SegmentRetirementQueueManager(bool runLocally)
    {
      Log.LogInformation("Establishing segment retirement queue cache context");

      // Get the ignite grid and cache references
      ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
      ICache<ISegmentRetirementQueueKey, TAGFileBufferQueueItem> queueCache = ignite.GetCache<ISegmentRetirementQueueKey, TAGFileBufferQueueItem>(TRexCaches.SegmentRetirementQueueCacheName());

      // Create a thread to periodically 

      Log.LogInformation("Completed segment retirement queue manager initialisation");
    }

    public void Dispose()
    {
      ignite?.Dispose();
    }
  }
}
