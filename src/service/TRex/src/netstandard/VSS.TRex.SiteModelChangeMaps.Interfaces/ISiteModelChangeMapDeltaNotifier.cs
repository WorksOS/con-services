using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps.Interfaces
{
  public interface ISiteModelChangeMapDeltaNotifier
  {
    void Notify(Guid projectUID, ISubGridTreeBitMask changeMap, SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation);
  }
}
