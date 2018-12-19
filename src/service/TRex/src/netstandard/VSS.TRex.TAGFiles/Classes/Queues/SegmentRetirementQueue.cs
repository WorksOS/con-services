using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  /// <summary>
  /// Implements a queue of segments requiring 'retiring' from the cache. Each retiree has an expiry date at which it
  /// will be removed. This date is intended to be beyond the eventual consistency requirements of any active query in 
  /// progress on the immutable data grid.
  /// </summary>
  public class SegmentRetirementQueue : ISegmentRetirementQueue
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SegmentRetirementQueue>();

    private ICache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> QueueCache;

    public void Add(ISegmentRetirementQueueKey key, SegmentRetirementQueueItem value)
    {
      Log.LogInformation($"Adding {value.SegmentKeys?.Length} retirees to queue for project {key.ProjectUID}");

      QueueCache.Put(key, value);
    }

    /// <summary>
    /// Constructs a segment retirement queue for the given ignite grid.
    /// </summary>
    public SegmentRetirementQueue()
    {
      IIgnite ignite = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);

      QueueCache = ignite.GetOrCreateCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(
        new CacheConfiguration
        {
          Name = TRexCaches.SegmentRetirementQueueCacheName(),

          // Replicate the maps across nodes
          CacheMode = CacheMode.Partitioned,

//          AffinityFunction = new MutableNonSpatialAffinityFunction(),

          // No backups for now
          Backups = 0,
          KeepBinaryInStore = true,

          //QueryEntities = new[]
          //{
          //  new QueryEntity(typeof(ISegmentRetirementQueueKey), typeof(SegmentRetirementQueueItem))
          //}
        });
    }

    /// <summary>
    /// Finds all the items in the retirement queue ready for removal and returns them
    /// </summary>
    /// <param name="earlierThan"></param>
    /// <returns></returns>
    public void Remove(DateTime earlierThan)
    {
      // Do it the simple scan query way
      try
      {
        var filter = new SegmentRetirementQueueQueryFilter(earlierThan.Ticks);
        var query = new ScanQuery<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>
        {
          Filter = filter,
          Local = true
        };

        List<ISegmentRetirementQueueKey> toRemove = QueueCache.Query(query).GetAll().Select(x => x.Key).ToList();

        Log.LogInformation($"Removing {toRemove.Count} retirement groups from retirement queue cache");

        QueueCache.RemoveAll(toRemove);

        Log.LogInformation($"Removed {toRemove.Count} retirement groups from retirement queue cache");
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(Remove)} experienced exception while removing retirees from retirement queue:");
      }
    }

    /// <summary>
    /// Finds all the items in the retirement queue ready for removal and returns them
    /// </summary>
    /// <param name="earlierThan"></param>
    /// <returns></returns>
    public List<SegmentRetirementQueueItem> Query(DateTime earlierThan)
    {
      // Do it the simple scan query way
      try
      {
        var filter = new SegmentRetirementQueueQueryFilter(earlierThan.Ticks);
        var query = new ScanQuery<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>
        {
          Filter = filter,
          Local = true
        };
        return QueueCache.Query(query).GetAll().Select(x => x.Value).ToList();
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(Query)} experienced exception while querying retirees:");
        return null;
      }

      /*  var sql = new SqlQuery(typeof(SegmentRetirementQueueItem), $"_KEY.InsertUTCAsLong < {earlierThan.Ticks}")
        {
          Local = true
        };
  
        Log.LogInformation($"Retirement queue SQL string is {sql.Sql}");
        Log.LogInformation($"Retirement queue SQL string is {sql}");
  
        try
        {
          return QueueCache.Query(sql).Select(x => x.Value).ToList();
        }
        catch (Exception e)
        {
          Log.LogError(e, $"{nameof(Query)} experienced exception while querying retirees:");
          return null;
        }
  */
    }
  }
}
