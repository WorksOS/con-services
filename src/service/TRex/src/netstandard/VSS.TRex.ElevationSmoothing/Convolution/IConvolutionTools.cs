using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionTools<T>
  {
    void Convolve(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver);

    void Convolve(T[,] source, T[,] dest, IConvolver<T> convolver);
  }
}
