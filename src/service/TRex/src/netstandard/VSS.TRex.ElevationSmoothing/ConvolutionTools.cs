using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionTools<T> : BaseConvolutionTools<T>
  {
    public override void Smooth(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver, T nullValue)
    {
      var context = new ConvolutionSubGridContext<GenericLeafSubGrid<T>, T>(leaf, nullValue);

      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => context.Value(x, y), (x, y, v) => smoothedLeaf.Items[x, y] = v, context.NullValue);
    }


    public void Smooth(T[,] source, T[,] dest, int contextSize)
    {
      //      for (var i = 0, limiti = source.GetDimension(); i < SubGridTreeConsts.SubGridTreeDimension; i++)

    }
  }
}
