using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    public class TAGFileBufferQueueItemHandler : IDisposable
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

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
//        private EventWaitHandle waitHandle;

        /// <summary>
        /// The grouper responsible for grouping TAG files into Project/Asset groups ready for processing into a
        /// project.
        /// </summary>
        private TAGFileBufferQueueGrouper grouper;

        private IIgnite ignite;
        private ICache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> queueCache;

        private List<Guid> ProjectsToAvoid = new List<Guid>();

        private void ProcessTAGFilesFromGrouper()
        {
            Log.LogInformation("ProcessTAGFilesFromGrouper starting executing");

            ITAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey();

            // Cycle looking for new work to do as TAG files arrive until aborted...
            do
            {
                var hadWorkToDo = false;

                // Check to see if there is a work package to feed to the processing pipeline
                // -> Ask the grouper for a package 
                var package = grouper.Extract(ProjectsToAvoid, out Guid projectID)?.ToList();
                int packageCount = package?.Count ?? 0;

                if (packageCount > 0)
                {
                    Log.LogInformation($"Extracted package from grouper, ProjectID:{projectID}, with {packageCount} items");

                    hadWorkToDo = true;

                    try
                    {
                        List<TAGFileBufferQueueItem> TAGQueueItems = null;
                        List<ProcessTAGFileRequestFileItem> fileItems = null;
                        try
                        {
                            TAGQueueItems = package?.Select(x =>
                            {
                                try
                                {
                                    return queueCache.Get(x);
                                }
                                catch (KeyNotFoundException e)
                                {
                                    // Odd, but let's be graceful and attempt to process the remainder in the package
                                    Log.LogError($"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                                    return null;
                                }
                                catch (Exception e)
                                {
                                    // More worrying, report and bail on this package
                                    Log.LogError($"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
                                    throw;
                                }
                            }).ToList();

                            fileItems = TAGQueueItems
                                .Where(x => x != null)
                                .Select(x => new ProcessTAGFileRequestFileItem
                                {
                                    FileName = x.FileName,
                                    TagFileContent = x.Content,
                                    IsJohnDoe = x.IsJohnDoe
                                }).ToList();
                        }
                        catch (Exception e)
                        {
                            Log.LogError($"Error, exception {e} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
                        }

                        if (TAGQueueItems?.Count > 0)
                        {
                            // -> Supply the package to the processor
                            ProcessTAGFileRequest request = new ProcessTAGFileRequest();
                            ProcessTAGFileResponse response = request.Execute(new ProcessTAGFileRequestArgument
                            {
                                ProjectID = projectID,
                                AssetID = TAGQueueItems[0].AssetID,
                                TAGFiles = fileItems
                            });

                            removalKey.ProjectID = projectID;
                            removalKey.AssetID = TAGQueueItems[0].AssetID;

                            // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                            foreach (var tagFileResponse in response.Results)
                            {
                                try
                                {
                                    // TODO: Determine what to do in this failure more: - Leave in place? Copy to dead letter queue? Place in S3 bucket pending downstream handling?
                                    if (!tagFileResponse.Success)
                                        Log.LogInformation($"Grouper1 TAG file {tagFileResponse.FileName} successfully processed");
                                    else
                                        Log.LogError($"Grouper1 TAG file failed to process, with exception {tagFileResponse.Exception}. WARNING: FILE REMOVED FROM QUEUE");

                                    removalKey.FileName = tagFileResponse.FileName;

                                    if (!queueCache.Remove(removalKey))
                                    {
                                        Log.LogError($"Failed to remove TAG file {removalKey}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.LogError(
                                        $"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectID} from the TAG file buffer queue");
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Remove the project from the avoid list
                        Log.LogInformation($"Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectID} from [{(!ProjectsToAvoid.Any() ? "Empty" : ProjectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                        grouper.RemoveProjectFromAvoidList(ProjectsToAvoid, projectID);
                    }
                }

                // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                if (!hadWorkToDo)
                {
                    //Log.LogInformation($"ProcessTAGFilesFromGrouper sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                    Thread.Sleep(kTAGFileBufferQueueServiceCheckIntervalMS);
                    //waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                }
            } while (!aborted);

            Log.LogInformation("ProcessTAGFilesFromGrouper completed executing");
        }

        /// <summary>
        /// Contains the business logic for managing the processing of a package of TAG files into TRex
        /// The package of TAG files contains files for a single project [and currently a single machine]
        /// </summary>
        /// <param name="package"></param>
        private void ProcessTAGFileBucketFromGrouper2(IReadOnlyList<ITAGFileBufferQueueKey> package)
        {
            Guid projectID = package[0].ProjectID;

            List<TAGFileBufferQueueItem> TAGQueueItems = null;
            List<ProcessTAGFileRequestFileItem> fileItems = null;
            try
            {
                TAGQueueItems = package.Select(x =>
                {
                    try
                    {
                        return queueCache.Get(x);
                    }
                    catch (KeyNotFoundException e)
                    {
                        // Odd, but let's be graceful and attempt to process the remainder in the package
                        Log.LogError(
                            $"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache");
                        return null;
                    }
                    catch (Exception e)
                    {
                        // More worrying, report and bail on this package
                        Log.LogError(
                            $"Error, exception {e} occurred while attempting to retrieve TAG file for key {x} from the TAG file buffer queue cache - aborting processing this package");
                        throw;
                    }
                }).ToList();
                fileItems = TAGQueueItems
                    .Where(x => x != null)
                    .Select(x => new ProcessTAGFileRequestFileItem
                    {
                        FileName = x.FileName,
                        TagFileContent = x.Content,
                        IsJohnDoe = x.IsJohnDoe
                    }).ToList();
            }
            catch (Exception e)
            {
                Log.LogError(
                    $"Error, exception {e} occurred while attempting to retrieve TAG files from the TAG file buffer queue cache");
            }

            if (TAGQueueItems?.Count > 0)
            {
                // -> Supply the package to the processor
                ProcessTAGFileRequest request = new ProcessTAGFileRequest();
                ProcessTAGFileResponse response = request.Execute(new ProcessTAGFileRequestArgument
                {
                    ProjectID = projectID,
                    AssetID = TAGQueueItems[0].AssetID,
                    TAGFiles = fileItems
                });

                ITAGFileBufferQueueKey removalKey = new TAGFileBufferQueueKey
                {
                    ProjectID = projectID,
                    AssetID = TAGQueueItems[0].AssetID
                };

                // -> Remove the set of processed TAG files from the buffer queue cache (depending on processing status?...)
                foreach (var tagFileResponse in response.Results)
                {
                    try
                    {
                        // TODO: Determine what to do in this failure mode: Leave in place? Copy to dead letter queue? Place in S3 bucket pending downstream handling?
                        if (tagFileResponse.Success)
                            Log.LogInformation($"Grouper2 TAG file {tagFileResponse.FileName} successfully processed");
                        else
                            Log.LogError($"Grouper2 TAG file failed to process, with exception {tagFileResponse.Exception}. WARNING: FILE REMOVED FROM QUEUE");

                        removalKey.FileName = tagFileResponse.FileName;

                        if (!queueCache.Remove(removalKey))
                            Log.LogError($"Failed to remove TAG file {removalKey}");
                    }
                    catch (Exception e)
                    {
                        Log.LogError(
                            $"Exception {e} occurred while removing TAG file {tagFileResponse.FileName} in project {projectID} from the TAG file buffer queue");
                    }
                }
            }
        }

        /// <summary>
        /// A version of ProcessTAGFilesFromGrouper2 that uses task parallelism
        /// </summary>
        private void ProcessTAGFilesFromGrouper2()
        {
            try
            {
                Log.LogInformation("#In# ProcessTAGFilesFromGrouper2 starting executing");

                // Cycle looking for new work to do as TAG files arrive until aborted...
                do
                {
                    var hadWorkToDo = false;

                    // Check to see if there is a work package to feed to the processing pipeline
                    // -> Ask the grouper for a package 
                    var package = grouper.Extract(ProjectsToAvoid, out Guid projectID)?.ToList();
                    int packageCount = package?.Count ?? 0;

                    if (packageCount > 0)
                    {
                        Log.LogInformation(
                            $"Extracted package from grouper, ProjectID:{projectID}, with {packageCount} items in thread {Thread.CurrentThread.ManagedThreadId}");

                        hadWorkToDo = true;
                        try
                        {
                            //Task task = Task.Factory.StartNew(() => ProcessTAGFileBucketFromGrouper2(package));
                            //task.Wait();

                            Log.LogInformation(
                                $"#Progress# Start processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                            ProcessTAGFileBucketFromGrouper2(package);
                            Log.LogInformation(
                                $"#Progress# Completed processing {packageCount} TAG files from package in thread {Thread.CurrentThread.ManagedThreadId}");
                        }
                        finally
                        {
                            // Remove the project from the avoid list
                            Log.LogInformation(
                                $"#Progress# Thread {Thread.CurrentThread.ManagedThreadId}: About to remove project {projectID} from [{(!ProjectsToAvoid.Any() ? "Empty" : ProjectsToAvoid.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                            grouper.RemoveProjectFromAvoidList(ProjectsToAvoid, projectID);
                        }
                    }

                    // if there was no work to do in the last epoch, sleep for a bit until the next check epoch
                    if (!hadWorkToDo)
                    {
                        //Log.LogInformation($"ProcessTAGFilesFromGrouper2 sleeping for {kTAGFileBufferQueueServiceCheckIntervalMS}ms");

                        Thread.Sleep(kTAGFileBufferQueueServiceCheckIntervalMS);
                        //waitHandle.WaitOne(kTAGFileBufferQueueServiceCheckIntervalMS);
                    }
                } while (!aborted);

                Log.LogInformation("#Out# ProcessTAGFilesFromGrouper2 completed executing");
            }
            catch (Exception e)
            {
                Log.LogError($"Exception {e} thrown in ProcessTAGFilesFromGrouper2");
            }
        }

        /// <summary>
        /// No-arg constructor that creates the internal grouper, thread and wait handle for managing incoming TAG files
        /// into the cache and supplied by the continuous query
        /// </summary>
        public TAGFileBufferQueueItemHandler()
        {
            ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            queueCache = ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

            // Create the grouper responsible for grouping TAG files into project/asset combinations
            grouper = new TAGFileBufferQueueGrouper();
            // waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            const int NumTasks = 1;
            Task[] tasks = Enumerable.Range(0, NumTasks).Select(x => Task.Factory.StartNew(ProcessTAGFilesFromGrouper2, TaskCreationOptions.LongRunning)).ToArray();
        }

        /// <summary>
        /// Adds a new TAG file item from the buffer queue via the remote filter supplied tot he continuous query
        /// </summary>
        /// <param name="key"></param>
        public void Add(ITAGFileBufferQueueKey key)
        {
            grouper.Add(key);
        }

        public void Dispose()
        {
            aborted = true;
            //waitHandle?.Set();
            //waitHandle?.Dispose();
            //waitHandle = null;
        }
    }
}
