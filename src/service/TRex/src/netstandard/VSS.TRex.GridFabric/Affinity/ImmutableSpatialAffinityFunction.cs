using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// The affinity function used by TRex to spread spatial data amongst processing servers
    /// </summary>
    public class ImmutableSpatialAffinityFunction : AffinityFunctionBase
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

            if (!(key is ISubGridSpatialAffinityKey))
            {
                Log.LogInformation($"Unknown key type to compute spatial affinity partition key for: {key}");
                throw new ArgumentException($"Unknown key type to compute spatial affinity partition key for: {key}");
            }

            ISubGridSpatialAffinityKey value = (ISubGridSpatialAffinityKey)key;

            // Compute partition number against the subgrid location in the spatial affinity key
            return (int)SubGridCellAddress.ToSpatialPartitionDescriptor(value.SubGridX, value.SubGridY);
        }
    }
}
