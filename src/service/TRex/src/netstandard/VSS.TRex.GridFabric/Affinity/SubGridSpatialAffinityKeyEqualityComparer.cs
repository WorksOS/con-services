using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  public class SubGridSpatialAffinityKeyEqualityComparer : IEqualityComparer<ISubGridSpatialAffinityKey>
  {
    public bool Equals(ISubGridSpatialAffinityKey x, ISubGridSpatialAffinityKey y)
    {
      if (ReferenceEquals(x, y))
        return true;

      return x != null && y != null &&
             x.ProjectUID.Equals(y.ProjectUID) &&
             x.SubGridX == y.SubGridX &&
             x.SubGridY == y.SubGridY &&
             x.SegmentIdentifier.Equals(y.SegmentIdentifier);
    }

    public int GetHashCode(ISubGridSpatialAffinityKey obj) => obj.GetHashCode();
  }
}
