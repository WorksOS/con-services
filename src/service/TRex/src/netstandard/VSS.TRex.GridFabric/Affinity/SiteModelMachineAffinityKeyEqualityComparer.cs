using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SiteModelMachineAffinityKeyEqualityComparer : IEqualityComparer<ISiteModelMachineAffinityKey>
  {
    public bool Equals(ISiteModelMachineAffinityKey x, ISiteModelMachineAffinityKey y)
    {
      if (ReferenceEquals(x, y))
        return true;

      return x != null && y != null &&
             x.ProjectUID.Equals(y.ProjectUID) && x.AssetUID.Equals(y.AssetUID) && x.StreamType == y.StreamType;
    }

    public int GetHashCode(ISiteModelMachineAffinityKey obj) => obj.GetHashCode();
  }
}
