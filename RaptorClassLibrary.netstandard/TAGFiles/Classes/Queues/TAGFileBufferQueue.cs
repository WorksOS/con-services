﻿using Apache.Ignite.Core;
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
    /*
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
    */

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

//        private IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle;

        /// <summary>
        /// Creates or obtains a reference to an already created TAG file buffer queue
        /// </summary>
        private void InstantiateCache()
        {
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());

            QueueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

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

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
//            queryHandle = QueueCache.QueryContinuous
//            (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener())
//                { Filter = new RemoteTAGFileFilter() },
//             initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>());
        }

        /// <summary>
        /// Executes business logic to select a set of TAG files from the cache and submit it for processing.
        /// By default it will choose a set of TAG files in the cache where the project and asset IDs match
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> SelectBatch()
        {
            IEnumerable<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> localItems = QueueCache.GetLocalEntries(CachePeekMode.Primary);

            // ReSharper disable once PossibleMultipleEnumeration
            ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem> first = localItems?.FirstOrDefault();

            if (first == null) // There's no work to do!
                return null;

            // Get the list of all TAG files in the buffer matching the project and asset IDs of the first item
            // in the list, limiting the result set to 100 TAG files.
            // ReSharper disable once PossibleMultipleEnumeration
                List<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> candidates = localItems
                    .Take(1000)
                    .Where(x => x.Value.ProjectUID == first.Value.ProjectUID && x.Value.AssetUID == first.Value.AssetUID)
                    .Take(100)
                    .ToList();

            return candidates;
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