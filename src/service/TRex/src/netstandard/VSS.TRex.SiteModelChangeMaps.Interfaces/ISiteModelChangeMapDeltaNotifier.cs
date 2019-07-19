using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps.Interfaces
{
  public interface ISiteModelChangeMapDeltaNotifier
  {
    /// <summary>
    /// Creates a change map buffer queue item and places it in to the cache for the service processor to collect
    /// </summary>
    void Notify(Guid projectUID, DateTime InsertUTC, ISubGridTreeBitMask changeMap, SiteModelChangeMapOrigin origin, SiteModelChangeMapOperation operation);
  }
}
