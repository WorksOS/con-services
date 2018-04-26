using System;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using log4net;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Responsible for management of querying the TAG file buffer queue for work to do.
    /// Utilises Ignite continuous queries
    /// </summary>
    public class TAGFileBufferQueueManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The query handle created by the continuous query. Used to get the initial scan query handle and 
        /// to dispose the continuous query when no longer needed.
        /// </summary>
        private IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle;

        /// <summary>
        /// Local Ignite resource reference
        /// </summary>
        private IIgnite ignite;

        /// <summary>
        /// No-arg constructor. Instantiates the continouus query and performs initial scan of elements that the remote filter 
        /// will populate into the node-local groupers within the mutable grid./
        /// </summary>
        public TAGFileBufferQueueManager()
        {
            Log.Info("Establishing Ignite and TAG file buffer queue cache contexts");

            // Get the ignite grid and cache references
            ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            RemoteTAGFileFilter TAGFileFilter = new RemoteTAGFileFilter();

            Log.Info("Creating continuous query");

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener())
                    {
                        Local = false,
                        Filter = TAGFileFilter
                    },
                    initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
                    {
                        Local = false,
                        Filter = TAGFileFilter
                    });

            // Perform the initial query to grab all existing elements and add them to the grouper
            // All processing shoudl happen on the remote node in the implementation of the TAGFileFilter remote filter
            foreach (var item in queryHandle.GetInitialQueryCursor())
            {
                Log.Error(
                    $"A cache entry ({item.Key}) from the TAG file buffer queue was passed back to the local scan query rather than intercepted by the remote filter fiulre");
            }

            Log.Info("Completed TAG file buffer queue manager initialisation");
        }

        public void Dispose()
        {
            queryHandle?.Dispose();
            ignite?.Dispose();
        }
    }
}
