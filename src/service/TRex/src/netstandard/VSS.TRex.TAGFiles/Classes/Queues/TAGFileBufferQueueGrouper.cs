using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
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
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The maximum number of TAG files the grouper will permit in a bucket of TAG file before being committed to the 
        /// full buckets list.
        /// </summary>
        public const int kMaxNumberOfTAGFilesPerBucket = 100;

        /// <summary>
        /// GroupMap is a dictionary (keyed on project UID) of dictionaries (keyed on AssetUID) of
        /// TAG files to be processed for that projectUID/assetUID combination 
        /// </summary>
        private Dictionary<Guid, Dictionary<Guid, List<ITAGFileBufferQueueKey>>> groupMap;

        /// <summary>
        /// fullBuckets is a list of arrays of TAG files where each array is a collection of TAG files for a
        /// particular asset/project combination. New arrays of these keys are added as the groupMap dictionary
        /// for a project/assert ID combination hits a critical limit (eg: 100 TAG files)
        /// </summary>
        private List<ITAGFileBufferQueueKey[]> fullBuckets;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TAGFileBufferQueueGrouper()
        {
            groupMap = new Dictionary<Guid, Dictionary<Guid, List<ITAGFileBufferQueueKey>>>();
            fullBuckets = new List<ITAGFileBufferQueueKey[]>();
        }

        /// <summary>
        /// Adds another TAG file buffer queue key into the tracked groups for processing
        /// </summary>
        /// <param name="key"></param>
        public void Add(ITAGFileBufferQueueKey key)
        {
            lock (this)
            {
                if (groupMap.TryGetValue(key.ProjectUID, out Dictionary<Guid, List<ITAGFileBufferQueueKey>> assetsDict))
                {
                    if (!assetsDict.TryGetValue(key.AssetUID, out List<ITAGFileBufferQueueKey> keyList))
                    {
                        assetsDict.Add(key.AssetUID, new List<ITAGFileBufferQueueKey> { key });
                    }
                    else
                    {
                        keyList.Add(key);

                        // Check if this bucket is full
                        if (keyList.Count >= kMaxNumberOfTAGFilesPerBucket)
                        {
                            fullBuckets.Add(keyList.ToArray());
                            assetsDict.Remove(key.AssetUID);
                        }
                    }
                }
                else
                {
                    groupMap.Add(key.ProjectUID, new Dictionary<Guid, List<ITAGFileBufferQueueKey>> { { key.AssetUID, new List<ITAGFileBufferQueueKey> {key} } });
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
            if (fullBuckets.Count > 0)
            {
                foreach (var bucket in fullBuckets)
                    if (bucket.Any())
                    {
                        if (avoidProjects != null && avoidProjects.Any(x => x == bucket[0].ProjectUID))
                            continue;

                        selectedProject = bucket[0].ProjectUID;
                        return true;
                    }
            }

            foreach (Guid projectID in groupMap.Keys)
            {
                // Check the project is not in the avoid list
                if (avoidProjects != null && avoidProjects.Any(x => x == projectID))
                    continue;

                // Check the project has grouped TAG files for an asset
                if (groupMap[projectID].Keys.Count > 0)
                {
                    selectedProject = projectID;
                    return true;
                }
            }

            selectedProject = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Returns a list of TAG files for a project and asset within the set of project/asset pairs 
        /// maintained in the grouper. The caller may provide an 'avoid' list of projects which the grouper
        /// will ignore when locating a group of TAG files to return.
        /// The returned list of TAG file keys is then removed from the grouper. It is the responsibility
        /// of the caller to ensure the TAG files are processed and removed from the overall TAGFileBufferQueue,
        /// and any appropriate failure mode handling while processing the bucket of TAG files returned.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITAGFileBufferQueueKey> Extract(List<Guid> avoidProjects, out Guid projectID)
        {
            lock (this)
            {
                // Choose an appropriate project and return the first set of asset grouped TAG files for it
                if (!SelectProject(avoidProjects, out projectID))
                    return null;

                Guid _projectID = projectID;
                IEnumerable<ITAGFileBufferQueueKey> result;

                // Determine if there is a full bucket for the requested project
                int resultIndex = fullBuckets.FindIndex(x => x[0].ProjectUID == _projectID);
                if (resultIndex >= 0)
                {
                    result = fullBuckets[resultIndex];
                    fullBuckets.RemoveAt(resultIndex);
                }
                else
                {
                    // No full buckets - extract the first list of asset based TAG files from the grouper for the selected project
                    result = groupMap[projectID].Values.First();
                    groupMap[projectID].Remove(result.First().AssetUID);
                }

                if (result.Any())
                {
                    // Add the project to the avoid list
                    Log.LogInformation($"Thread {Thread.CurrentThread.ManagedThreadId}: About to add project {projectID} to [{(!avoidProjects.Any() ? "Empty" : avoidProjects.Select(x => $"{x}").Aggregate((a, b) => $"{a} + {b}"))}]");
                    avoidProjects.Add(projectID);
                }

                return result;
            }
        }

        /// <summary>
        /// Performs an interlock removal of the project from the project to avoid list
        /// </summary>
        /// <param name="avoidProjects"></param>
        /// <param name="projectID"></param>
        public void RemoveProjectFromAvoidList(List<Guid> avoidProjects, Guid projectID)
        {
            lock (this)
            {
                avoidProjects.Remove(projectID);
            }
        }
    }
}
