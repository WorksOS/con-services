using Apache.Ignite.Core.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
    /// <summary>
    /// Service metaphor providing access andmanagement control over designs stored for site models
    /// </summary>
    [Serializable]
    public class TAGFileBufferQueueService : IService, ITAGFileBufferQueueService
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            Log.Info($"TAGFileBufferQueueService {context.Name} initialising");
        }

        /// <summary>
        /// Executes the life cycle of the service until it is aborted
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IServiceContext context)
        {
            Log.Info($"TAGFileBufferQueueService {context.Name} starting executing");

            aborted = false;
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            List<long> ProjectsToAvoid = new List<long>();

            // Get the ignite grid and cache references

            IIgnite _ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());

            if (_ignite == null)
            {
                Log.Error("Ignite reference in service is null - aborting service execution");
                return;
            }

            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache =
                _ignite?.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            TAGFileBufferQueueItemHandler handler = TAGFileBufferQueueItemHandler.Instance();

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            using (IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener()) { Local = true },
                 initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem> { Local = true }))
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

            Log.Info($"TAGFileBufferQueueService {context.Name} completed executing");
        }

        /// <summary>
        /// Cancels the current operation context of the service
        /// </summary>
        /// <param name="context"></param>
        public void Cancel(IServiceContext context)
        {
            Log.Info($"TAGFileBufferQueueService {context.Name} cancelling");

            aborted = true;
            waitHandle?.Set();
        }
    }
}
