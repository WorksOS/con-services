using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.GridFabric.Caches;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Implements a queue of segments requiring 'retiring' from the cache. Each retiree has an expiry date at which it
    /// will be removed. This date is intended to be beyond the eventual consistency requirements of any active query in 
    /// progress on the immutable data grid.
    /// </summary>
    public class SegmentRetirementQueue
    {
        private ICache<long, SegmentRetirementQueueItem> QueueCache;

        private void Add(DateTime date, string value)
        {
            QueueCache.Put(date.ToBinary(), new SegmentRetirementQueueItem(date.ToBinary(), value));
        }

        public SegmentRetirementQueue(string gridName)
        {
            IIgnite Ignite = Ignition.GetIgnite(gridName);

            QueueCache = Ignite.GetOrCreateCache<long, SegmentRetirementQueueItem>(
                new CacheConfiguration
                {
                    Name = TRexCaches.SegmentRetirementQueueCacheName(),
                    QueryEntities = new[] 
                    {
                        new QueryEntity(typeof(long), typeof(SegmentRetirementQueueItem))
                    },
                    KeepBinaryInStore = true
                });
        }

        public IEnumerable<SegmentRetirementQueueItem> Query(DateTime earlierThan)
        {
            var sql = new SqlQuery(typeof(SegmentRetirementQueueItem), $"_key < {earlierThan.ToBinary()}");

            try
            {
                return QueueCache.Query(sql).Select(x => x.Value).ToArray();
            }
            catch
            {
                return null;
            }
        }

/*
Add(DateTime.Now, "First");
Add(DateTime.Now, "Second");
Add(DateTime.Now, "Third");
Add(DateTime.Now, "Fourth");
Add(DateTime.Now, "Fifth");
*/ 
    }
}
