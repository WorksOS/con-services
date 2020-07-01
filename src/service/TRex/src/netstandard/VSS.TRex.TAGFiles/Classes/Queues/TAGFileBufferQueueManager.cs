using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Responsible for management of querying the TAG file buffer queue for work to do.
    /// Utilizes Ignite continuous queries and needs to be instantiated in context, unlike the grid deployed service model
    /// </summary>
    public class TAGFileBufferQueueManager : IDisposable
    {
        private static readonly ILogger _log = Logging.Logger.CreateLogger<TAGFileBufferQueueManager>();

        /// <summary>
        /// The query handle created by the continuous query. Used to get the initial scan query handle and
        /// to dispose the continuous query when no longer needed.
        /// </summary>
        private readonly IContinuousQueryHandle<ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>> _queryHandle;

        /// <summary>
        /// Local Ignite resource reference
        /// </summary>
        private readonly IIgnite _ignite;

        /// <summary>
        /// No-arg constructor. Instantiates the continuous query and performs initial scan of elements that the remote filter 
        /// will populate into the node-local groupers within the mutable grid.
        /// </summary>
        public TAGFileBufferQueueManager(bool runLocally)
        {
            _log.LogInformation("Establishing Ignite and TAG file buffer queue cache contexts");

            // Get the ignite grid and cache references
            _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());
            var queueCache = _ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());
            var handler = new TAGFileBufferQueueItemHandler();
            var tagFileFilter = new RemoteTAGFileFilter(handler);

            _log.LogInformation("Creating continuous query");

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continuous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            _queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener(handler))
                    {
                        Local = runLocally,
                        Filter = tagFileFilter
                    },
                    initialQry: new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>
                    {
                        Local = runLocally,
                        Filter = tagFileFilter
                    });

            // Perform the initial query to grab all existing elements and add them to the grouper
            // All processing should happen on the remote node in the implementation of the TAGFileFilter remote filter
            foreach (var item in _queryHandle.GetInitialQueryCursor())
            {
                _log.LogError(
                    $"A cache entry ({item.Key}) from the TAG file buffer queue was passed back to the local scan query rather than intercepted by the remote filter");
            }

            _log.LogInformation("Completed TAG file buffer queue manager initialization");
        }

        public void Dispose()
        {
            _queryHandle?.Dispose();
            _ignite?.Dispose();
        }
    }
}
