using System;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps
{
    /// <summary>
    /// A static class facade around requests for existence maps related to designs and surveyed surfaces
    /// </summary>
    public class ExistenceMaps : IExistenceMaps
  {
        public void SetExistenceMap(INonSpatialAffinityKey key, ISubGridTreeBitMask mask) => SetExistenceMapRequest.Execute(key, mask);
        public void SetExistenceMap(Guid siteModelID, long descriptor, Guid ID, ISubGridTreeBitMask mask) => SetExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public ISubGridTreeBitMask GetSingleExistenceMap(INonSpatialAffinityKey key) => GetSingleExistenceMapRequest.Execute(key);
        public ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID) => GetSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public ISubGridTreeBitMask GetCombinedExistenceMap(INonSpatialAffinityKey[] keys) => GetCombinedExistenceMapRequest.Execute(keys);
        public ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys) => GetCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}
