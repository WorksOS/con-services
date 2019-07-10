using System.Collections.Generic;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;

namespace VSS.TRex.SiteModelChangeMaps
{
  public class SiteModelChangeBufferQueueKeyEqualityComparer : IEqualityComparer<ISiteModelChangeBufferQueueKey>
  {
    public bool Equals(ISiteModelChangeBufferQueueKey x, ISiteModelChangeBufferQueueKey y)
    {
      if (ReferenceEquals(x, y))
        return true;

      return x != null && y != null &&
             x.ProjectUID.Equals(y.ProjectUID) && x.InsertUTCTicks.Equals(y.InsertUTCTicks);
    }

    public int GetHashCode(ISiteModelChangeBufferQueueKey obj) => obj.GetHashCode();
  }
}
