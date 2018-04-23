using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using log4net;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Queues;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Represents a buffered queue of TAG files awaiting processing. The queue of TAG files is stored in a 
    /// partitioned Ignite cache based on the ProjectUID
    /// </summary>
    public class TAGFileBufferQueue
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Ignite cache reference that holds the TAG files. This cache is keyed on the TAG file name and uses the
        /// ProjectUID field in the queue item to control affinity placement of the TAG files themselves
        /// </summary>
        private ICache<string, TAGFileBufferQueueItem> QueueCache;

        /// <summary>
        /// Creates or obtains a reference to an already created TAG file buffer queue
        /// </summary>
        private void InstantiateCache()
        {
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());

            try
            {
                QueueCache = ignite.GetCache<string, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());
            }
            catch // Exception is thrown if the cache does not exist
            {
                QueueCache = ignite.GetOrCreateCache <string, TAGFileBufferQueueItem> (
                    new CacheConfiguration
                    {
                        Name = RaptorCaches.TAGFileBufferQueueCacheName(),
                        QueryEntities = new[]
                        {
                            new QueryEntity(typeof(string), typeof(TAGFileBufferQueueItem))
                        },

                        KeepBinaryInStore = true,

                        // Replicate the maps across nodes
                        CacheMode = CacheMode.Partitioned,

                        // No backups for now
                        Backups = 0,

                        DataRegionName = DataRegions.TAG_FILE_BUFFER_QUEUE_DATA_REGION
                    });
            }

            if (QueueCache == null)
            {
                Log.Info($"Failed to get or create Ignite cache {RaptorCaches.TAGFileBufferQueueCacheName()}");
                throw new ArgumentException("Ignite cache not available");
            }
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFileBufferQueue()
        {
            InstantiateCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TAGFileBufferQueueItem> GetProcessingBatch()
        {
            //ICacheEntryFilter
            //var cursor = QueueCache.Query(new ScanQuery<string, TAGFileBufferQueueItem>(new QueryFilter()));

            return null;
        }
    }
}