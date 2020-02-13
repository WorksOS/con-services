using System;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public class BaseConvolutionTools<T> : IConvolutionTools<T>
  {
    public virtual void Smooth(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver, T nullValue)
    {
      throw new NotImplementedException("Smooth(GenericLeafSubGrid<T> leaf, ...) not implemented in this convolution tools class");
    }

    public virtual void Smooth(T[,] source, T[,] dest, IConvolver<T> convolver, T nullValue)
    {
      throw new NotImplementedException("Smooth(T[,] source, ...) not implemented in this convolution tools class");
    }
  }
}
