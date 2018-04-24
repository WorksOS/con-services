using Apache.Ignite.Core.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Services;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

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
            List<Guid> ProjectsToAvoid = new List<Guid>();

            // Get the ignite grid and cache references
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache =
                ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            using (IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener()) { Local = true },
                    initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem> { Local = true}))
            {
                // Cycle looking for work to do until aborted...
                do
                {
                    hadWorkToDo = false;
                    // Check to see if there is a work package to feed to the processing pipline
                    // -> Ask the grouper for a package 
                    var package = grouper.Extract(ProjectsToAvoid, out Guid projectUID);

                    if (package != null)
                    {
                        hadWorkToDo = true;

                        // Add the project to the avoid list
                        ProjectsToAvoid.Add(projectUID);

                        List<TAGFileBufferQueueItem> TAGQueueItems = null;
                        List<ProcessTAGFileRequestFileItem> fileItems = null;
                        try
                        {
                            TAGQueueItems = package.Select(x => queueCache.Get(x)).ToList();
                            fileItems = TAGQueueItems.Select(x =>
                                new ProcessTAGFileRequestFileItem
                                {
                                    FileName = x.FileName,
                                    TagFileContent = x.Content,
                                }).ToList();
                        }
                        catch (Exception E)
                        {
                            Log.Error(
                                $"Error, exception {E} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
                        }

                        // -> Supply the package to the processor
                        ProcessTAGFileRequest request = new ProcessTAGFileRequest();
                        ProcessTAGFileResponse response = request.Execute(new ProcessTAGFileRequestArgument
                        {
                            AssetUID = TAGQueueItems[0].AssetUID,
                            ProjectUID = projectUID,
                            TAGFiles = fileItems
                        });

                        // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                        foreach (var tagFileResponse in response.Results)
                        {
                            try
                            {
                                if (!tagFileResponse.Success)
                                {
                                    Log.Error(
                                        $"TAG file failed to process, with exception {tagFileResponse.Exception}");
                                }

                                queueCache.Remove(new TAGFileBufferQueueKey
                                {
                                    ProjectUID = projectUID,
                                    FileName = tagFileResponse.FileName
                                });
                            }
                            catch (Exception e)
                            {
                                Log.Error(
                                    $"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectUID} from the TAG file buffer queue");
                                throw;
                            }
                        }

                        // Remove the project from the avoid list
                        ProjectsToAvoid.Remove(projectUID);
                    }

                    // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                    if (!hadWorkToDo)
                    {
                        Log.Info(
                            $"TAGFileBufferQueueService {context.Name} sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                        waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                    }
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
            waitHandle.Set();
        }
    }
}
