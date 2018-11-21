using VSS.TRex.Designs.TTM.Optimised;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IOptimisedTTMProfilerFactory
  {
    IOptimisedTTMProfiler NewInstance(TrimbleTINModel ttm,
      IOptimisedSpatialIndexSubGridTree index,
      int[] indices);
  }
}
