using System;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ExistenceMaps
{
    /// <summary>
    /// A static class facade around the existence map requests
    /// </summary>
    public static class ExistenceMaps
    {
        public static void SetExistenceMap(NonSpatialAffinityKey key, SubGridTreeSubGridExistenceBitMask mask) => SetExistenceMapRequest.Execute(key, mask);
        public static void SetExistenceMap(Guid siteModelID, long descriptor, Guid ID, SubGridTreeSubGridExistenceBitMask mask) => SetExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(NonSpatialAffinityKey key) => GetSingleExistenceMapRequest.Execute(key);
        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID) => GetSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(NonSpatialAffinityKey[] keys) => GetCombinedExistenceMapRequest.Execute(keys);
        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys) => GetCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}
