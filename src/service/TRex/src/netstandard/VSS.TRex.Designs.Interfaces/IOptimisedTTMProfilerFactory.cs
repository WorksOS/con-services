using VSS.TRex.Designs.Interfaces;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling.Interfaces
{
  public interface IOptimisedTTMProfilerFactory
  {
    IOptimisedTTMProfiler NewInstance(TrimbleTINModel ttm,
      IOptimisedSpatialIndexSubGridTree index,
      int[] indices);
  }
}
