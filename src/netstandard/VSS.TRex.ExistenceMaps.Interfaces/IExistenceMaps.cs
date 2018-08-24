using System;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.Interfaces
{
  public interface IExistenceMaps
  {
    void SetExistenceMap(NonSpatialAffinityKey key, ISubGridTreeBitMask mask);
    void SetExistenceMap(Guid siteModelID, long descriptor, Guid ID, ISubGridTreeBitMask mask);
    ISubGridTreeBitMask GetSingleExistenceMap(NonSpatialAffinityKey key);
    ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID);
    ISubGridTreeBitMask GetCombinedExistenceMap(NonSpatialAffinityKey[] keys);
    ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys);
  }
}
