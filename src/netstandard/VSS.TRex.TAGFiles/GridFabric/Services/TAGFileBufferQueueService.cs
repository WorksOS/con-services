using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
    /// <summary>
    /// Service metaphor providing access and management control over designs stored for site models
    /// </summary>
    [Serializable]
    public class TAGFileBufferQueueService : IService, ITAGFileBufferQueueService
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The interval between epochs where the service checks to see if there is anything to do
        /// </summary>
        private const int kTAGFileBufferQueueServiceCheckIntervalMS = 1000;

        /// <summary>
        /// Flag set then Cancel() is called to instruct the service to finish operations
        /// </summary>
        [NonSerialized]
        private bool aborted;

        /// <summary>
        /// The event wait handle used to mediate sleep periods between operation epochs of the service
        /// </summary>
        [NonSerialized]
        private EventWaitHandle waitHandle;

        /// <summary>
        /// Default no-args constructor that tailors this service to apply to TAG processign node in the mutable data grid
        /// </summary>
        public TAGFileBufferQueueService()
        {
        }

        /// <summary>
        /// Initialises the service ready for accessing buffered TAG files and providing them to processing contexts
        /// </summary>
        /// <param name="context"></param>
        public void Init(IServiceContext context)
        {
            Log.LogInformation($"TAGFileBufferQueueService {context.Name} initialising");
        }

        /// <summary>
        /// Executes the life cycle of the service until it is aborted
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IServiceContext context)
        {
            Log.LogInformation($"TAGFileBufferQueueService {context.Name} starting executing");

            aborted = false;
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            // Get the ignite grid and cache references

            IIgnite _ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());

            if (_ignite == null)
            {
                Log.LogError("Ignite reference in service is null - aborting service execution");
                return;
            }

            ICache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache =
                _ignite?.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

            TAGFileBufferQueueItemHandler handler = TAGFileBufferQueueItemHandler.Instance();

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continuous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            using (IContinuousQueryHandle<ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener()) { Local = true },
                 initialQry: new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> { Local = true }))
            {
                // Perform the initial query to grab all existing elements and add them to the grouper
                foreach (var item in queryHandle.GetInitialQueryCursor())
                {
                     handler.Add(item.Key);
                }

                // Cycle looking for new work to do as TAG files arrive until aborted...
                do
                {
                    waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                } while (!aborted);
            }

            Log.LogInformation($"TAGFileBufferQueueService {context.Name} completed executing");
        }

        /// <summary>
        /// Cancels the current operation context of the service
        /// </summary>
        /// <param name="context"></param>
        public void Cancel(IServiceContext context)
        {
            Log.LogInformation($"TAGFileBufferQueueService {context.Name} cancelling");

            aborted = true;
            waitHandle?.Set();
        }
    }
}
