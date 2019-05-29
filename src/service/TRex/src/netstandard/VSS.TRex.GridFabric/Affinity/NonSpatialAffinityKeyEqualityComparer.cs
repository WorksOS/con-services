using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class NonSpatialAffinityKeyEqualityComparer : IEqualityComparer<INonSpatialAffinityKey>
  {
    public bool Equals(INonSpatialAffinityKey x, INonSpatialAffinityKey y)
    {
      return x.ProjectUID.Equals(y.ProjectUID) && x.KeyName.Equals(y.KeyName);
    }

    public int GetHashCode(INonSpatialAffinityKey obj) => obj.GetHashCode();
  }
}
