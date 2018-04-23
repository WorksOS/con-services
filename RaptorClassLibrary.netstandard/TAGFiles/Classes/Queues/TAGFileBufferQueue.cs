using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apache.Ignite.Core.Cache.Event;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
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
    /// The grouper accepts individual keys representing TAG files in the TAG file buffer queue and groups it with
    /// TAG files to be processed with the smae project and asste. To reduce storage, only the key values are grouped,
    /// the values may be requested from the cache at the time the TAG file is processed.
    /// </summary>
    public class TAGFileBufferQueueGrouper
    {
        private const int kMaxNumberOfTAGFilesPerBucket = 100;

        /// <summary>
        /// GroupMap is a dictionary (keyed on project UID) of dictionaries (keyes on AssetUID) of
        /// TAG files to be processed for that projectUID/assetUID combination 
        /// </summary>
        private Dictionary<Guid, Dictionary<Guid, List<TAGFileBufferQueueKey>>> groupMap;

        /// <summary>
        /// fullBuckets is a list of arrays of TAG files where each array is a collection of TAG files for a
        /// particular asset/project combination. New arrays of these keys are added as the groupMap dictionary
        /// for a project/assert ID combination hits a critical limit (eg: 100 TAG files)
        /// </summary>
        private List<TAGFileBufferQueueKey[]> fullBuckets;

        /// <summary>
        /// Defaultno-arg constructor
        /// </summary>
        public TAGFileBufferQueueGrouper()
        {
            groupMap = new Dictionary<Guid, Dictionary<Guid, List<TAGFileBufferQueueKey>>>();
            fullBuckets = new List<TAGFileBufferQueueKey[]>();
        }

        /// <summary>
        /// Adds another TAG file buffer queue key into the tracked groups for processing
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TAGFileBufferQueueKey key, TAGFileBufferQueueItem value)
        {
            lock (this)
            {
                if (groupMap.TryGetValue(key.ProjectUID, out Dictionary<Guid, List<TAGFileBufferQueueKey>> assetsDict))
                {
                    if (!assetsDict.TryGetValue(value.AssetUID, out List<TAGFileBufferQueueKey> keyList))
                    {
                        keyList = new List<TAGFileBufferQueueKey> {key};
                        assetsDict.Add(value.AssetUID, keyList);
                    }
                    else
                    {
                        keyList.Add(key);
                    }

                    // Check if this bucket is full
                    if (keyList.Count > kMaxNumberOfTAGFilesPerBucket)
                    {
                        fullBuckets.Add(keyList.ToArray());
                        assetsDict.Remove(value.AssetUID);
                    }
                }
                else
                {
                    Dictionary<Guid, List<TAGFileBufferQueueKey>> newDict = new Dictionary<Guid, List<TAGFileBufferQueueKey>> {{value.AssetUID, new List<TAGFileBufferQueueKey>{ key }}};

                    groupMap.Add(key.ProjectUID, newDict);
                }
            }
        }
        
        /// <summary>
        /// Returns a list of TAG files for a project and asset
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TAGFileBufferQueueKey> Extract(Guid projectUID, Guid assetUID)
        {
            lock (this)
            {
                //groupMap.Remove()
                //groupMap.Keys.FirstOrDefault()?.
                return null;
            }
        }
    }

    public class RemoteTAGFileFilter : ICacheEntryEventFilter<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        public bool Evaluate(ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem> evt)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. That context needs to be available through some kind of static namespace, such as the
            // TAG file processor context.
            // ....

            // Advise the caller this item is not filtered [as have already dealth with it so no futher 
            // processing of the item is required.
            return false;

            // Currently accept all TAG files on the primary node
        }
    }

    public class RemoteTAGFileListener : ICacheEntryEventListener<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
    {
        public void OnEvent(IEnumerable<ICacheEntryEvent<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> evts)
        {
            // Add the keys for the given events into the Project/Asset mapping buckets ready for a processing context
            // to acquire them. That context needs to be available through some kind of static namespace, such as the
            // TAG file processor context.
            // ....
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

        private IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle;

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

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            queryHandle = QueueCache.QueryContinuous
            (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new RemoteTAGFileListener())
                { Filter = new RemoteTAGFileFilter() },
             initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>());
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