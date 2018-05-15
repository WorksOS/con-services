using System;
using VSS.TRex.TAGFiles.Classes.Queues;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by Raptor to spread spatial data amongst processing servers
    /// </summary>
    [Serializable]
    public class MutableNonSpatialAffinityFunction : AffinityFunctionBase
    {
        /// <summary>
        /// Given a cache key, determine which partition the cache item should reside
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override int GetPartition(object key)
        {
            // Pull the subgrid origin location for the subgrid or segment represented in the cache key and calculate the 
            // spatial processing division descriptor to use as the partition affinity key

            if (key is NonSpatialAffinityKey affinityKey)
            {
                // Compute partition number as the modulo NumPartitions result against the project iD in the spatial affinity key
                return Math.Abs(affinityKey.ProjectID.GetHashCode()) % Partitions;
            }

            if (key is TAGFileBufferQueueKey bufferQueueyKey)
            {
                // Compute partition number as the modulo NumPartitions result against the project iD in the spatial affinity key
                return Math.Abs(bufferQueueyKey.ProjectID.GetHashCode()) % Partitions;
            }

            Log.Info($"Unknown key type to compute non spatial affinity partition key for: {key}");
            throw new ArgumentException($"Unknown key type to compute non spatial affinity partition key for: {key}");
        }
    }
}
