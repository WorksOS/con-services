using System;
using Microsoft.Extensions.Logging;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeMapDeltaNotifier>();

    private readonly IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem> _queueCache;

    public SiteModelChangeMapDeltaNotifier()
    {
      _queueCache = DIContext.Obtain<Func<IStorageProxyCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>()();
    }

    /// <summary>
    /// Creates a change map buffer queue item and places it in to the cache for the service processor to collect
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="insertUtc"></param>
    /// <param name="changeMap"></param>
    /// <param name="origin"></param>
    /// <param name="operation"></param>
    public void Notify(Guid projectUid, DateTime insertUtc, ISubGridTreeBitMask changeMap, SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation)
    {
      Log.LogInformation($"Adding site model change map notification for project {projectUid}");

      _queueCache.Put(new SiteModelChangeBufferQueueKey(projectUid, insertUtc), 
        new SiteModelChangeBufferQueueItem
        {
          ProjectUID = projectUid,
          InsertUTC = insertUtc,
          Operation = operation,
          Origin = origin,
          Content = changeMap?.ToBytes()
        });
    }
  }
}
