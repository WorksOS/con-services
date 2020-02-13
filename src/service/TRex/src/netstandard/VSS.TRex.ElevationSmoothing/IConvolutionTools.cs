using VSS.TRex.SubGridTrees;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionTools<T>
  {
    void Smooth(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver, T nullValue);

    void Smooth(T[,] source, T[,] dest, IConvolver<T> convolver, T nullValue);
  }
}
