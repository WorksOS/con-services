using System;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionToolsBase<T> : IConvolutionTools<T>
  {
    public virtual void Convolve(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver)
    {
      throw new NotImplementedException("Convolve(GenericLeafSubGrid<T> leaf, ...) not implemented in this convolution tools class");
    }

    public virtual void Convolve(T[,] source, T[,] dest, IConvolver<T> convolver)
    {
      throw new NotImplementedException("Convolve(T[,] source, ...) not implemented in this convolution tools class");
    }
  }
}
