using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// The grouper accepts individual keys representing TAG files in the TAG file buffer queue and groups it with
    /// TAG files to be processed with the same project and asset. To reduce storage, only the key values are grouped,
    /// the values may be requested from the cache at the time the TAG file is processed.
    /// </summary>
    public class TAGFileBufferQueueGrouper
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueueGrouper>();

        /// <summary>
        /// The maximum number of TAG files the grouper will permit in a bucket of TAG file before being committed to the 
        /// full buckets list.
        /// </summary>
        private static readonly int MaxNumberOfTagFilesPerBucket = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH", Consts.MAX_GROUPED_TAG_FILES_TO_PROCESS_PER_PROCESSING_EPOCH);

        /// <summary>
        /// GroupMap is a dictionary (keyed on project UID) of
        /// TAG files to be processed for that projectUID/assetUID combination 
        /// </summary>
        private readonly Dictionary<Guid, List<ITAGFileBufferQueueKey>> _groupMap;

        /// <summary>
        /// fullBuckets is a list of arrays of TAG files where each array is a collection of TAG files for a
        /// particular asset/project combination. New arrays of these keys are added as the groupMap dictionary
        /// for a project/assert ID combination hits a critical limit (eg: 100 TAG files)
        /// </summary>
        private readonly List<ITAGFileBufferQueueKey[]> _fullBuckets;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFileBufferQueueGrouper()
        {
            _groupMap = new Dictionary<Guid, List<ITAGFileBufferQueueKey>>();
            _fullBuckets = new List<ITAGFileBufferQueueKey[]>();
        }

        /// <summary>
        /// Adds another TAG file buffer queue key into the tracked groups for processing
        /// </summary>
        /// <param name="key"></param>
        public void Add(ITAGFileBufferQueueKey key)
        {
            lock (this)
            {
                if (Log.IsTraceEnabled())
                { 
                    Log.LogTrace($"Grouper adding TAG file {key.FileName} representing asset {key.AssetUID} within project {key.ProjectUID} into an appropriate group");
                }

                if (_groupMap.TryGetValue(key.ProjectUID, out var keyList))
                {
                    // Check if this bucket is full
                    if (keyList.Count >= MaxNumberOfTagFilesPerBucket)
                    {
                      _fullBuckets.Add(keyList.ToArray());
                      keyList.Clear();
                    }

                    keyList.Add(key);
                }
                else
                {
                    _groupMap.Add(key.ProjectUID, new List<ITAGFileBufferQueueKey> { key } );
                }
            }
        }

        /// <summary>
        /// Selects a project from the project/asset group maps that is not contained in the avoidProjects
        /// list, and which has non-zero number TAG file groups.
        /// </summary>
        /// <param name="avoidProjects"></param>
        /// <param name="selectedProject"></param>
        /// <returns></returns>
        private bool SelectProject(List<Guid> avoidProjects, out Guid selectedProject)
        {
            // Preferentially selected a project from the full buckets list
            if (_fullBuckets.Count > 0)
            {
                foreach (var bucket in _fullBuckets)
                { 
                    if (bucket.Any())
                    {
                        if (avoidProjects != null && avoidProjects.Any(x => x == bucket[0].ProjectUID))
                        { 
                            continue;
                        }

                        selectedProject = bucket[0].ProjectUID;
                        return true;
                    }
                }
            }

            foreach (var projectId in _groupMap.Keys)
            {
                // Check the project is not in the avoid list
                if (avoidProjects != null && avoidProjects.Any(x => x == projectId))
                { 
                    continue;
                }

                // Check the project has grouped TAG files for an asset
                if (_groupMap[projectId].Count > 0)
                {
                    selectedProject = projectId;
                    return true;
                }
            }

            selectedProject = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Returns a list of TAG files for a project within the set of project pairs 
        /// maintained in the grouper. The caller may provide an 'avoid' list of projects which the grouper
        /// will ignore when locating a group of TAG files to return.
        /// The returned list of TAG file keys is then removed from the grouper. It is the responsibility
        /// of the caller to ensure the TAG files are processed and removed from the overall TAGFileBufferQueue,
        /// and any appropriate failure mode handling while processing the bucket of TAG files returned.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITAGFileBufferQueueKey> Extract(List<Guid> avoidProjects, out Guid projectId)
        {
            lock (this)
            {
                // Choose an appropriate project and return the first set of grouped TAG files for it
                if (!SelectProject(avoidProjects, out projectId))
                {
                    return null;
                }

                var localProjectId = projectId;
                IEnumerable<ITAGFileBufferQueueKey> result;

                // Determine if there is a full bucket for the requested project
                var resultIndex = _fullBuckets.FindIndex(x => x[0].ProjectUID == localProjectId);
                if (resultIndex >= 0)
                {
                    result = _fullBuckets[resultIndex];
                    _fullBuckets.RemoveAt(resultIndex);
                }
                else
                {
                    // No full buckets - extract the first list of asset based TAG files from the grouper for the selected project
                    result = _groupMap[projectId];
                    _groupMap.Remove(projectId);
                }

                if (result.Any())
                {
                    // Add the project to the avoid list
                    if (Log.IsTraceEnabled())
                    {
                        Log.LogTrace($"Thread {Thread.CurrentThread.ManagedThreadId}: About to add project {projectId} to [{(!avoidProjects.Any() ? "Empty" : avoidProjects.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                    }

                    avoidProjects.Add(projectId);
                }

                Log.LogInformation($"Grouper returning group containing {result.Count()} TAG files");

                if (Log.IsTraceEnabled())
                {
                    var count = 0;
                    foreach (var tagFile in result)
                    {
                        Log.LogTrace($"Returned TAG file {count++} is {tagFile.FileName} representing asset {tagFile.AssetUID} within project {tagFile.ProjectUID}");
                    }
                }
                
                return result;
            }
        }

        /// <summary>
        /// Performs an interlock removal of the project from the project to avoid list
        /// </summary>
        /// <param name="avoidProjects"></param>
        /// <param name="projectId"></param>
        public void RemoveProjectFromAvoidList(List<Guid> avoidProjects, Guid projectId)
        {
            lock (this)
            {
                avoidProjects.Remove(projectId);
            }
        }
    }
}
