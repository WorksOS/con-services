using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionTools<T> : BaseConvolutionTools<T>
  {
    public override void Smooth(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver)
    {
      var context = new ConvolutionSubGridContext<GenericLeafSubGrid<T>, T>(leaf, convolver.Accumulator.NullValue);

      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => context.Value(x, y), (x, y, v) => smoothedLeaf.Items[x, y] = v);
    }


    public void Smooth(T[,] source, T[,] dest, int contextSize)
    {
      //      for (var i = 0, limiti = source.GetDimension(); i < SubGridTreeConsts.SubGridTreeDimension; i++)

    }
  }
}
