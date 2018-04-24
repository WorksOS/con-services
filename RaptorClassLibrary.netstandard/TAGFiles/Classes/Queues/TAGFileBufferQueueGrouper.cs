using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// The grouper accepts individual keys representing TAG files in the TAG file buffer queue and groups it with
    /// TAG files to be processed with the smae project and asste. To reduce storage, only the key values are grouped,
    /// the values may be requested from the cache at the time the TAG file is processed.
    /// </summary>
    public class TAGFileBufferQueueGrouper
    {
        private const int kMaxNumberOfTAGFilesPerBucket = 100;

        /// <summary>
        /// GroupMap is a dictionary (keyed on project UID) of dictionaries (keyed on AssetUID) of
        /// TAG files to be processed for that projectUID/assetUID combination 
        /// </summary>
        private Dictionary<Guid, Dictionary<Guid, List<TAGFileBufferQueueKey>>> groupMap;

        /// <summary>
        /// fullBuckets is a list of arrays of TAG files where each array is a collection of TAG files for a
        /// particular asset/project combination. New arrays of these keys are added as the groupMap dictionary
        /// for a project/assert ID combination hits a critical limit (eg: 100 TAG files)
        /// </summary>
        private List<TAGFileBufferQueueKey[]> fullBuckets;

        /// <summary>
        /// Defaultno-arg constructor
        /// </summary>
        public TAGFileBufferQueueGrouper()
        {
            groupMap = new Dictionary<Guid, Dictionary<Guid, List<TAGFileBufferQueueKey>>>();
            fullBuckets = new List<TAGFileBufferQueueKey[]>();
        }

        /// <summary>
        /// Adds another TAG file buffer queue key into the tracked groups for processing
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TAGFileBufferQueueKey key, TAGFileBufferQueueItem value)
        {
            lock (this)
            {
                if (groupMap.TryGetValue(key.ProjectUID, out Dictionary<Guid, List<TAGFileBufferQueueKey>> assetsDict))
                {
                    if (!assetsDict.TryGetValue(value.AssetUID, out List<TAGFileBufferQueueKey> keyList))
                    {
                        keyList = new List<TAGFileBufferQueueKey> {key};
                        assetsDict.Add(value.AssetUID, keyList);
                    }
                    else
                    {
                        keyList.Add(key);
                    }

                    // Check if this bucket is full
                    if (keyList.Count > kMaxNumberOfTAGFilesPerBucket)
                    {
                        fullBuckets.Add(keyList.ToArray());
                        assetsDict.Remove(value.AssetUID);
                    }
                }
                else
                {
                    Dictionary<Guid, List<TAGFileBufferQueueKey>> newDict =
                        new Dictionary<Guid, List<TAGFileBufferQueueKey>>
                        {
                            {value.AssetUID, new List<TAGFileBufferQueueKey> {key}}
                        };

                    groupMap.Add(key.ProjectUID, newDict);
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
            foreach (Guid projectGuid in groupMap.Keys)
            {
                // Check the project is not in the avoid list
                if (avoidProjects != null && avoidProjects.Any(x => x == projectGuid))
                    continue;

                // Check the project has grouped TAG files for an asset
                if (groupMap[projectGuid].Keys.Count > 0)
                {
                    selectedProject = projectGuid;
                    return true;
                }
            }

            selectedProject = default(Guid);
            return false;
        }

        /// <summary>
        /// Returns a list of TAG files for a project and asset within the set of project/asset pairs 
        /// maintined in the grouper. The caller may provide an 'avoid' list of projects which the grouper
        /// will ignore when locating a group of TAG files to return
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TAGFileBufferQueueKey> Extract(List<Guid> avoidProjects, out Guid projectUID)
        {
            lock (this)
            {
                // Choose an appropriate project and return the first set of asset grouped TAG files for it
                return SelectProject(avoidProjects, out projectUID) ? groupMap[projectUID].Values.First() : null;
            }
        }
    }
}
