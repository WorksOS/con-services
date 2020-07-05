using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.Interfaces
{
  public interface IExistenceMapServer
  {
    /// <summary>
    /// Get a specific existence map given its key
    /// </summary>
    ISubGridTreeBitMask GetExistenceMap(INonSpatialAffinityKey key);
  }
}
