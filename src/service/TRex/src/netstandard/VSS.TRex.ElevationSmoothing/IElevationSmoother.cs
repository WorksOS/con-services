using VSS.TRex.SubGridTrees.Core;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IElevationSmoother
  {
    GenericSubGridTree_Float Smooth();
  }
}
