using System;
using VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.ExistenceMaps
{
    /// <summary>
    /// A static class facade around the existence map requests
    /// </summary>
    public static class ExistenceMaps
    {
        public static void SetExistenceMap(NonSpatialAffinityKey key, SubGridTreeSubGridExistenceBitMask mask) => SetExistenceMapRequest.Execute(key, mask);
        public static void SetExistenceMap(Guid siteModelID, long descriptor, long ID, SubGridTreeSubGridExistenceBitMask mask) => SetExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(NonSpatialAffinityKey key) => GetSingleExistenceMapRequest.Execute(key);
        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, long ID) => GetSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(NonSpatialAffinityKey[] keys) => GetCombinedExistenceMapRequest.Execute(keys);
        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, long>[] keys) => GetCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}
