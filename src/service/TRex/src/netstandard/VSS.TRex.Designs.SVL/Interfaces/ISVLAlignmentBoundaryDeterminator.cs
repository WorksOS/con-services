using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.Interfaces
{
  public interface ISVLAlignmentBoundaryDeterminator
  {
    bool DetermineBoundary(out DesignProfilerRequestResult calcResult, out Fence fence);
  }
}
