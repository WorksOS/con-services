using System;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.Interfaces
{
  public interface IExistenceMaps
  {
    void SetExistenceMap(INonSpatialAffinityKey key, ISubGridTreeBitMask mask);
    void SetExistenceMap(Guid siteModelID, long descriptor, Guid ID, ISubGridTreeBitMask mask);
    ISubGridTreeBitMask GetSingleExistenceMap(INonSpatialAffinityKey key);
    ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID);
    ISubGridTreeBitMask GetCombinedExistenceMap(INonSpatialAffinityKey[] keys);
    ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys);
  }
}
