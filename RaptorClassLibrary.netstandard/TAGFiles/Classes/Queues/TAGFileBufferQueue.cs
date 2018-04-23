using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    [Serializable]
    public class TAGFileBufferQueueQueryFilter : ICacheEntryFilter<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        public bool Invoke(ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem> entry)
        {
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            var QueueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            ICacheAffinity affinity = ignite.GetAffinity(RaptorCaches.TAGFileBufferQueueCacheName());

           // affinity.GetAffinityKey<>()<Guid>(entry.Value.ProjectUID);

            throw new NotImplementedException();
        }

        public TAGFileBufferQueueQueryFilter()
        {

        }
    }

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
        /// The key is a string that 
        /// </summary>
        private ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> QueueCache;

        /// <summary>
        /// Creates or obtains a reference to an already created TAG file buffer queue
        /// </summary>
        private void InstantiateCache()
        {
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());

            try
            {
                QueueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());
            }
            catch // Exception is thrown if the cache does not exist
            {
                QueueCache = ignite.GetOrCreateCache <TAGFileBufferQueueKey, TAGFileBufferQueueItem> (
                    new CacheConfiguration
                    {
                        Name = RaptorCaches.TAGFileBufferQueueCacheName(),                       

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
        /// Executes business logic to select a set of TAG files from the cache and submit it for processing.
        /// By default it will choose a set of TAG files in the cache where the project and asset IDs match
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TAGFileBufferQueueItem> ProcessBatch()
        {
            IEnumerable<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> localItems = QueueCache.GetLocalEntries(new [] {CachePeekMode.Primary});

            ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem> first = localItems.FirstOrDefault();

            if (first != null)
            {
                // Get the list of all TAG files in the buffer matching the project and asset IDs of the first item
                // in the list, limiting the result set to 100 TAG files.
                List<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> candidates = localItems
                    .Where(x => x.Value.ProjectUID == first.Value.ProjectUID && x.Value.AssetUID == first.Value.AssetUID)
                    .Take(100)
                    .ToList();

                if (candidates?.Count > 0)
                {
                    // Submit the list of TAG files to the processor
                    // ...
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a new TAG file to the buffer queue.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>If an element with this key already exists in the cache this method will false, true otherwise</returns>
        public bool Add(TAGFileBufferQueueKey key, TAGFileBufferQueueItem value)
        {
            return QueueCache.PutIfAbsent(key, value);
        }
    }
}