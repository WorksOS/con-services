using VSS.TRex.SubGridTrees;

namespace VSS.TRex.DataSmoothing
{
  public interface IConvolutionTools
  {

  }

  public interface IConvolutionTools<T> : IConvolutionTools
  {
    void Convolve(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver);

    void Convolve(T[,] source, T[,] dest, IConvolver<T> convolver);
  }
}
