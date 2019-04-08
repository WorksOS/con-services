using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Queues
{
    [ExcludeFromCodeCoverage]
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

    [ExcludeFromCodeCoverage]
    public class TestQueueHolder
    {
        private ICache<long, TestQueueItem> QueueCache;

        private void Add(DateTime date, string value)
        {
            long ticks = date.Ticks;
            QueueCache.Put(ticks, new TestQueueItem(ticks, value));
        }

        public TestQueueHolder()
        {
            //  MutableClientServer Server = new MutableClientServer(new [] { "TestQueue2" });
            IIgnite Ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());
            QueueCache = Ignite.GetOrCreateCache<long, TestQueueItem>(
                new CacheConfiguration
                {
                    Name = "TestQueueCache2",
                    QueryEntities = new[] {
                        new QueryEntity(typeof(long), typeof(TestQueueItem))
                    },
                    KeepBinaryInStore = true
                });

            Add(DateTime.UtcNow, "First");
            Add(DateTime.UtcNow, "Second");
            Add(DateTime.UtcNow, "Third");
            Add(DateTime.UtcNow, "Fourth");
            Add(DateTime.UtcNow, "Fifth");
        }

        public IEnumerable<TestQueueItem> Query(DateTime earlierThan)
        {
            var sql = new SqlQuery(typeof(TestQueueItem), $"_key < {earlierThan.Ticks.ToString()}");
            var cursor = QueueCache.Query(sql);

            return cursor.Select(x => x.Value).ToArray();

            // foreach (var cacheEntry in cursor) {}
        }
    }

}
