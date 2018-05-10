using System;
using Apache.Ignite.Core.Cache.Affinity;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.GridFabric.Affinity
{
    /// <summary>
    /// The key type used to drive non-spatial affinity key mapping for elements stored in the Ignite cache. This controls
    /// which nodes in the PSNode layer the data for this key should reside. 
    /// </summary>
    [Serializable]
    public struct NonSpatialAffinityKey
    {
        /// <summary>
        /// A numeric ID for the project the subgrid data belongs to.
        /// </summary>
        public long ProjectID { get; set; }

        /// <summary>
        /// Name of the object in the cache, encoded as a string
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// A constructor for the affinity key that acccepts the project and subgrid origin location
        /// and returns an instance of the spatial affinity key
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="key"></param>
        public NonSpatialAffinityKey(long projectID, string key)
        {
            ProjectID = projectID;
            Key = key;
        }

        /// <summary>
        /// Converts the affinity key into a string representation suitable for use as a unique string
        /// identifying this data element in the cache.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{ProjectID}-{Key}";
    }
}
