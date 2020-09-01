using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.Interfaces
{
  public interface IExistenceMaps
  {
    ISubGridTreeBitMask GetSingleExistenceMap(Guid siteModelID, long descriptor, Guid ID);

    ISubGridTreeBitMask GetCombinedExistenceMap(Guid siteModelID, Tuple<long, Guid>[] keys);
  }
}
