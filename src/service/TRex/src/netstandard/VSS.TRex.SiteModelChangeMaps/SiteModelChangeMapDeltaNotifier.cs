using System;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.Storage.Interfaces;
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
      queueCache = DIContext.Obtain<IStorageProxyCache<ISiteModelChangeBufferQueueKey, SiteModelChangeBufferQueueItem>>();
    }

    /// <summary>
    /// Creates a change map buffer queue item and places it in to the cache for the service processor to collect
    /// </summary>
    /// <param name="projectUID"></param>
    /// <param name="insertUTC"></param>
    /// <param name="changeMap"></param>
    /// <param name="origin"></param>
    /// <param name="operation"></param>
    public void Notify(Guid projectUID, DateTime insertUTC, ISubGridTreeBitMask changeMap, SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation)
    {
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
