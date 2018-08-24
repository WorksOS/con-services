using System;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps
{
    /// <summary>
    /// A static class facade around the existence map requests
    /// </summary>
    public class ExistenceMaps : IExistenceMaps
  {
        public void SetExistenceMap(NonSpatialAffinityKey key, ISubGridTreeBitMask mask) => SetExistenceMapRequest.Execute(key, mask);
        public void SetExistenceMap(Guid siteModelID, long descriptor, Guid ID, ISubGridTreeBitMask mask) => SetExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public ISubGridTreeBitMask GetSingleExistenceMap(NonSpatialAffinityKey key) => GetSingleExistenceMapRequest.Execute(key);
        public ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID) => GetSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public ISubGridTreeBitMask GetCombinedExistenceMap(NonSpatialAffinityKey[] keys) => GetCombinedExistenceMapRequest.Execute(keys);
        public ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys) => GetCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}
