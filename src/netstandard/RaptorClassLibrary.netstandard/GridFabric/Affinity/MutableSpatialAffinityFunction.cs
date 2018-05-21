﻿using System;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by TRex to spread spatial data amongst processing servers
    /// </summary>
    [Serializable]
    public class MutableSpatialAffinityFunction : AffinityFunctionBase
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

            if (!(key is SubGridSpatialAffinityKey))
            {
                Log.LogInformation($"Unknown key type to compute spatial affinity partition key for: {key}");
                throw new ArgumentException($"Unknown key type to compute spatial affinity partition key for: {key}");
            }

            SubGridSpatialAffinityKey value = (SubGridSpatialAffinityKey)key;

            // Compute partition number as the modulo NumPartitions result against the project iD in the spatial affinity key
            return Math.Abs(value.ProjectID.GetHashCode()) % NumPartitions;
        }
    }
}
