using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using log4net;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class TAGFileBufferQueueItemHandler : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static TAGFileBufferQueueItemHandler _Instance = null;
        public static TAGFileBufferQueueItemHandler Instance() => _Instance ?? (_Instance = new TAGFileBufferQueueItemHandler());

        /// <summary>
        /// The interval between epochs where the service checks to see if there is anything to do
        /// </summary>
        private const int kTAGFileBufferQueueServiceCheckIntervalMS = 1000;

        /// <summary>
        /// Flag set then Cancel() is called to instruct the service to finish operations
        /// </summary>
        private bool aborted;

        /// <summary>
        /// The event wait handle used to mediate sleep periods between operation epochs of the service
        /// </summary>
        private EventWaitHandle waitHandle;

        /// <summary>
        /// The grouper responsible for grouping TAG files into Project/Asset groups ready for processing into a
        /// project.
        /// </summary>
        private TAGFileBufferQueueGrouper grouper;

        /// <summary>
        /// The thread providing independent lifecycle activity
        /// </summary>
        private Thread thread;

        private void ProcessTAGFilesFromGrouper()
        {
            Log.Info("ProcessTAGFilesFromGrouper starting executing");

            List<Guid> ProjectsToAvoid = new List<Guid>();

            // Get the ignite grid and cache references
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache =
                ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            RemoteTAGFileFilter TAGFileFilter = new RemoteTAGFileFilter();

            // Construct the continuous query machinery
            // Set the initial query to return all elements in the cache
            // Instantiate the queryHandle and start the continous query on the remote nodes
            // Note: Only cache items held on this local node will be handled here
            using (IContinuousQueryHandle<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryHandle = queueCache.QueryContinuous
                (qry: new ContinuousQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener())
                {
                    Local = false,
                    Filter = TAGFileFilter
                },
                initialQry: new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>
                {
                    Local = false,
                    Filter = TAGFileFilter
                }))
            {
                // Perform the initial query to grab all existing elements and add them to the grouper
                foreach (var item in queryHandle.GetInitialQueryCursor())
                {
                    grouper.Add(item.Key, item.Value);
                }

                // Cycle looking for new work to do as TAG files arrive until aborted...
                do
                {
                    var hadWorkToDo = false;

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
                            Log.Error($"Error, exception {E} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
                        }

                        if (TAGQueueItems?.Count > 0)
                        {
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
                                        Log.Error($"TAG file failed to process, with exception {tagFileResponse.Exception}");

                                    queueCache.Remove(new TAGFileBufferQueueKey
                                    {
                                        ProjectUID = projectUID,
                                        FileName = tagFileResponse.FileName
                                    });
                                }
                                catch (Exception e)
                                {
                                    Log.Error($"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectUID} from the TAG file buffer queue");
                                    throw;
                                }
                            }

                            // Remove the project from the avoid list
                            ProjectsToAvoid.Remove(projectUID);
                        }
                    }

                    // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                    if (!hadWorkToDo)
                    {
                        Log.Info($"ProcessTAGFilesFromGrouper sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                        waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                    }
                } while (!aborted);
            }

            Log.Info($"ProcessTAGFilesFromGrouper completed executing");
        }

        /// <summary>
        /// No-arg constructor that creates the intermal grouper, thread and waithandle for managing incoming TAG files
        /// into the cache and supplied by the continuous query
        /// </summary>
        public TAGFileBufferQueueItemHandler()
        {
            // Create the grouper responsible for grouping TAG files into projecft/asset combinations
            grouper = new TAGFileBufferQueueGrouper();
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            thread = new Thread(ProcessTAGFilesFromGrouper);
        }

        /// <summary>
        /// Adds a new TAG file item from the buffer queue via the remote filter supplied tot he continous query
        /// </summary>
        /// <param name="evt"></param>
        public void Add(TAGFileBufferQueueKey key, TAGFileBufferQueueItem value)
        {
            grouper.Add(key, value);
        }

        public void Dispose()
        {
            aborted = true;
            waitHandle?.Set();
            waitHandle?.Dispose();
            waitHandle = null;
        }
    }
}
