using System;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps
{
  /// <summary>
  /// Provides a notifier that allows a client to publish a delta against a site model change map.
  /// THe notifier abstracts the grid based cache that stores the change map deltas until the change map
  /// processor service handles them
  /// </summary>
  public class SiteModelChangeMapDeltaNotifier : ISiteModelChangeMapDeltaNotifier
  {
    private readonly IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem> queueCache;

    public SiteModelChangeMapDeltaNotifier()
    {
      queueCache = DIContext.Obtain<Func<IIgnite, IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>>()(DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable));
    }

    public void Notify(Guid projectUID, ISubGridTreeBitMask changeMap, SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation)
    {
      var insertUTC = DateTime.UtcNow;

      queueCache.Put(new SiteModelChangeBufferQueueKey(projectUID, insertUTC), 
        new SiteModelChangeBufferQueueItem
        {
          ProjectUID = projectUID,
          InsertUTC = insertUTC,
          Operation = operation,
          Origin = origin,
          Content = changeMap?.ToBytes()
        });

      queueCache.Commit();
    }
  }
}
