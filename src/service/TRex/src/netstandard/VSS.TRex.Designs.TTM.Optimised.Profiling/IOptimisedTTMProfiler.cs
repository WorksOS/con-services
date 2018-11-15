using System.Collections.Generic;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  public interface IOptimisedTTMProfiler
  {
    List<XYZS> Compute(XYZ[] points);
  }
}
