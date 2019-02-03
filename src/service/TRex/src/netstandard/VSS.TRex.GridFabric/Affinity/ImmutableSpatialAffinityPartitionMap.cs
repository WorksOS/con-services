using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;

namespace VSS.TRex.GridFabric.Affinity
{
    /// <summary>
    /// Defines a spatial cache partition map for the sub grid data maintained in the immutable data grid
    /// </summary>
    public class ImmutableSpatialAffinityPartitionMap : AffinityPartitionMap<ISubGridSpatialAffinityKey, byte[]>, IImmutableSpatialAffinityPartitionMap
    {
        /// <summary>
        /// Local static instance variable to hold the partition map singleton
        /// </summary>
        private static IImmutableSpatialAffinityPartitionMap _instance;

        /// <summary>
        /// Default no-args constructor that prepares a spatial affinity partition map for the immutable spatial caches
        /// </summary>
        public ImmutableSpatialAffinityPartitionMap() :
            base(Ignition.GetIgnite(TRexGrids.ImmutableGridName())
                .GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.ImmutableSpatialCacheName()))
        {
        }

        /// <summary>
        /// Instance method to return the partition mapper singleton instance
        /// </summary>
        /// <returns></returns>
        public static IImmutableSpatialAffinityPartitionMap Instance() 
          => _instance ?? (_instance = DIContext.Obtain<IImmutableSpatialAffinityPartitionMap>() ?? new ImmutableSpatialAffinityPartitionMap());
    }
}
