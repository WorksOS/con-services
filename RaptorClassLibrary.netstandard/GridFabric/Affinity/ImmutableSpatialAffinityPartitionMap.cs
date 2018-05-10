using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// Defines a spatial cache parition map for the subgrid data maintained in the immutable data grid
    /// </summary>
    public class ImmutableSpatialAffinityPartitionMap : AffinityPartitionMap<SubGridSpatialAffinityKey, byte[]>
    {
        /// <summary>
        /// Local static instance variable to hold the partition map singleton
        /// </summary>
        private static ImmutableSpatialAffinityPartitionMap _instance;

        /// <summary>
        /// Default no-args constructor that prepares a spatial affinity partition map for the immutable spatial caches
        /// </summary>
        public ImmutableSpatialAffinityPartitionMap() :
            base(Ignition.GetIgnite(RaptorGrids.RaptorImmutableGridName())
                .GetCache<SubGridSpatialAffinityKey, byte[]>(RaptorCaches.ImmutableSpatialCacheName()))
        {
        }

        /// <summary>
        /// Instance method to return the partition mapper singleton instance
        /// </summary>
        /// <returns></returns>
        public static ImmutableSpatialAffinityPartitionMap Instance() => _instance ?? (_instance = new ImmutableSpatialAffinityPartitionMap());
    }
}
