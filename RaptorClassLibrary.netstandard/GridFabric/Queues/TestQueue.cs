using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric.Queues
{
    [Serializable]
    public class TestQueueItem
    {
        /// <summary>
        /// A key field (a time) set up as an ordered (ascending) index
        /// </summary>
        [QuerySqlField(IsIndexed = true)]
        public long Date { get; set; }

        public string Value { get; set; }

        public TestQueueItem(long date, string value)
        {
            Date = date;
            Value = value;
        }
    }

    public class TestQueueHolder
    {
        private ICache<long, TestQueueItem> QueueCache;

        private void Add(DateTime date, string value)
        {
            QueueCache.Put(date.ToBinary(), new TestQueueItem(date.ToBinary(), value));
        }

        public TestQueueHolder()
        {
          //  RaptorMutableClientServer Server = new RaptorMutableClientServer(new [] { "TestQueue2" });
            IIgnite Ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            QueueCache = Ignite.GetOrCreateCache<long, TestQueueItem>(
                new CacheConfiguration
                {
                    Name = "TestQueueCache2",
                    QueryEntities = new[] {
                        new QueryEntity(typeof(long), typeof(TestQueueItem))
                    },
                    KeepBinaryInStore = true
                });

            Add(DateTime.Now, "First");
            Add(DateTime.Now, "Second");
            Add(DateTime.Now, "Third");
            Add(DateTime.Now, "Fourth");
            Add(DateTime.Now, "Fifth");
        }

        public IEnumerable<TestQueueItem> Query(DateTime earlierThan)
        {
            var sql = new SqlQuery(typeof(TestQueueItem), $"_key < {earlierThan.ToBinary().ToString()}");
            var cursor = QueueCache.Query(sql);

            return cursor.Select(x => x.Value).ToArray();

            // foreach (var cacheEntry in cursor) {}
        }
    }

}
