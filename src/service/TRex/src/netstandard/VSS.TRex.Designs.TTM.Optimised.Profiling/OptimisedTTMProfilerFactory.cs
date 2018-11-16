using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.TTM.Optimised.Profiling.Interfaces;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Creates an instance of an optimized TTM profiler from the TIN model and spatial index information
  /// </summary>
  public class OptimisedTTMProfilerFactory : IOptimisedTTMProfilerFactory
  {
    public IOptimisedTTMProfiler NewInstance(TrimbleTINModel ttm,
      IOptimisedSpatialIndexSubGridTree index,
      int[] indices) => new OptimisedTTMProfiler(ttm, index, indices);
  }
}
