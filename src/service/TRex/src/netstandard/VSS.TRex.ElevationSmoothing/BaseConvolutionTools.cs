using System;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public class BaseConvolutionTools<T> : IConvolutionTools<T>
  {
    public virtual void SmoothLeaf(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver, T nullValue)
    {
      throw new NotImplementedException("SmoothLeaf not implemented in this convolution tools class");
    }
  }
}
