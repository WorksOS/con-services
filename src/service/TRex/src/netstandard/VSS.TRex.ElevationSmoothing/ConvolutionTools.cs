using System;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionTools
  {
    void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf, IConvolver convolver);
  }

  public class BaseConvolutionTools : IConvolutionTools
  {
    public virtual void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf, IConvolver convolver)
    {
      throw new NotImplementedException("SmoothLeaf not implemented in this convolution tools class");
    }
  }

  public class ConvolutionTools : BaseConvolutionTools
  {
    public override void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf, IConvolver convolver)
    {
      var context = new ConvolutionSubGridContext<GenericLeafSubGrid_Float, float>(leaf, CellPassConsts.NullHeight);

      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => context.Value(x, y), (x, y, v) => smoothedLeaf.Items[x, y] = v, context.NullValue);
    }


    public void SmoothArray(float[,] source, float[,] dest, int contextSize)
    {
      //      for (var i = 0, limiti = source.GetDimension(); i < SubGridTreeConsts.SubGridTreeDimension; i++)

    }
  }
}
