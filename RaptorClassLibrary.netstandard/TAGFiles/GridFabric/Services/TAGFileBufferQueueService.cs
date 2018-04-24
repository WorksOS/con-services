using Apache.Ignite.Core.Services;
using log4net;
using System;
using System.Reflection;
using System.Threading;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Services;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
    /// <summary>
    /// Service metaphor providing access andmanagement control over designs stored for site models
    /// </summary>
    [Serializable]
    public class TAGFileBufferQueueService : BaseRaptorService, IService, ITAGFileBufferQueueService
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The interval between epochs where the service checks to see if there is anything to do
        /// </summary>
        private const int kTAGFileBufferQueueServiceCheckIntervalMS = 1000;

        [NonSerialized]
        private bool aborted;

        [NonSerialized]
        private TAGFileBufferQueueGrouper grouper;

        private EventWaitHandle waitHandle;

        /// <summary>
        /// Default no-args constructor that tailors this service to apply to TAG processign node in the mutable data grid
        /// </summary>
        public TAGFileBufferQueueService() : base(RaptorGrids.RaptorMutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
        {
        }

        /// <summary>
        /// Initialises the service ready for accessing buffered TAG files and providing them to processing contexts
        /// </summary>
        /// <param name="context"></param>
        public void Init(IServiceContext context)
        {
            Log.Info($"TAGFileBufferQueueService {context.Name} initialising");

            aborted = false;
            grouper = new TAGFileBufferQueueGrouper();
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the life cycle of the service until it is aborted
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IServiceContext context)
        {
            Log.Info($"TAGFileBufferQueueService {context.Name} starting executing");

            bool hadWorkToDo;

            // Create the Continous Query with an initial query
            // ...

            // Cycle looking for work to do until aborted...
            do
            {
                hadWorkToDo = false;
                // Check to see if there is a work package to feed to the processing pipline
                // -> Ask the grouper for a package
                // .....

                // -> Supply the paqckage to the processor
                // .....

                // -> Remove the set of processed TAG files from the buffer queue (depending on processing status?...)
                // .....

                // if there was no work to do in the last epoch, sleep for a bit until the next check epoch

                if (!hadWorkToDo)
                {
                    Log.Info($"TAGFileBufferQueueService {context.Name} sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                    waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                }
            } while (!aborted);

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
            waitHandle.Set();
        }
    }
}
