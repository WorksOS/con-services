using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionTools<T> : BaseConvolutionTools<T>
  {
    public override void Convolve(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver)
    {
      var context = new ConvolutionSubGridContext<GenericLeafSubGrid<T>, T>(leaf, convolver.Accumulator.NullValue);

      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => context.Value(x, y), (x, y, v) => smoothedLeaf.Items[x, y] = v);
    }


    public override void Convolve(T[,] source, T[,] dest, IConvolver<T> convolver)
    {
      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => source[x, y], (x, y, v) => dest[x, y] = v);
    }
  }
}
