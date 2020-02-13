using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionTools<T>
  {
    void SmoothLeaf(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver, T nullValue);
  }
}
