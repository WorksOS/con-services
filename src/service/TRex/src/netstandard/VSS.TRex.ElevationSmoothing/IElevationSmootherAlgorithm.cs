using VSS.TRex.SubGridTrees.Core;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IElevationSmootherAlgorithm
  {
    void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf, IConvolver convolve /*int contextSize*/);
  }
}
