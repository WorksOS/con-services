using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
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
    [NonSerialized] private static readonly ILogger Log = Logging.Logger.CreateLogger<SegmentRetirementQueue>();

    private ICache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem> QueueCache;

    public void Add(ISegmentRetirementQueueKey key, SegmentRetirementQueueItem value)
    {
      Log.LogInformation($"Adding {value.SegmentKeys?.Length} retirees to queue for project {key.ProjectID}");

      QueueCache.Put(key, value);
    }

    /// <summary>
    /// Constructs a segment retirement queue for the given ignite grid.
    /// </summary>
    /// <param name="ignite"></param>
    public SegmentRetirementQueue(IIgnite ignite)
    {
      QueueCache = ignite.GetOrCreateCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(
        new CacheConfiguration
        {
          Name = TRexCaches.SegmentRetirementQueueCacheName(),
          QueryEntities = new[]
          {
            new QueryEntity(typeof(ISegmentRetirementQueueKey), typeof(SegmentRetirementQueueItem))
          },
          KeepBinaryInStore = true
        });
    }

    /// <summary>
    /// Finds all the items in the retirement queue ready for removal and returns them
    /// </summary>
    /// <param name="earlierThan"></param>
    /// <returns></returns>
    public List<SegmentRetirementQueueItem> Query(DateTime earlierThan)
    {
      var sql = new SqlQuery(typeof(SegmentRetirementQueueItem), $"_key < {earlierThan.ToBinary()}")
      {
        Local = true
      };

      try
      {
        return QueueCache.Query(sql).Select(x => x.Value).ToList();
      }
      catch (Exception e)
      {
        Log.LogError($"{nameof(Query)} experienced exception while querying retirees: {e}");
        return null;
      }
    }
  }
}
