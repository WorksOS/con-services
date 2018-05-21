using System;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Responsible for management of querying the TAG file buffer queue for work to do.
    /// Utilises Ignite continuous queries and needs to be instantiated in context, unlike the grid deployed service model
    /// </summary>
    public class TAGFileBufferQueueManager : IDisposable
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

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
        public TAGFileBufferQueueManager(bool runLocally)
        {
            Log.LogInformation("Establishing Ignite and TAG file buffer queue cache contexts");

            // Get the ignite grid and cache references
            ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache = ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

            RemoteTAGFileFilter TAGFileFilter = new RemoteTAGFileFilter();

            Log.LogInformation("Creating continuous query");

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener())
                    {
                        Local = runLocally,
                        Filter = TAGFileFilter
                    },
                    initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
                    {
                        Local = runLocally,
                        Filter = TAGFileFilter
                    });

            // Perform the initial query to grab all existing elements and add them to the grouper
            // All processing shoudl happen on the remote node in the implementation of the TAGFileFilter remote filter
            foreach (var item in queryHandle.GetInitialQueryCursor())
            {
                Log.LogError(
                    $"A cache entry ({item.Key}) from the TAG file buffer queue was passed back to the local scan query rather than intercepted by the remote filter");
            }

            Log.LogInformation("Completed TAG file buffer queue manager initialisation");
        }

        public void Dispose()
        {
            queryHandle?.Dispose();
            ignite?.Dispose();
        }
    }
}
