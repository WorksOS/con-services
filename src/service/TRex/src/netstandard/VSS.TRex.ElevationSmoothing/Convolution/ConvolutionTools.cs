using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionTools<T> : ConvolutionToolsBase<T>
  {
    /// <summary>
    /// Implements convolution over a sub grid tree in <T>
    /// </summary>
    /// <param name="leaf"></param>
    /// <param name="smoothedLeaf"></param>
    /// <param name="convolver"></param>
    public override void Convolve(GenericLeafSubGrid<T> leaf, GenericLeafSubGrid<T> smoothedLeaf, IConvolver<T> convolver)
    {
      var context = new ConvolutionSubGridContext<GenericLeafSubGrid<T>, T>(leaf, convolver.Accumulator.NullValue);

      convolver.Convolve(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        (x, y) => context.Value(x, y), (x, y, v) => smoothedLeaf.Items[x, y] = v);
    }

    /// <summary>
    /// Implements convolution over a plain rectangular array of data values
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <param name="convolver"></param>
    public override void Convolve(T[,] source, T[,] dest, IConvolver<T> convolver)
    {
      var majorDimSource = source.GetLength(0);
      var minorDimSource = source.GetLength(1);

      var majorDimDest = dest.GetLength(0);
      var minorDimDest = dest.GetLength(1);

      if (majorDimSource != majorDimDest || minorDimSource != minorDimDest)
      {
        throw new ArgumentException("Dimensions of source and destination data are not the same");
      }
      
      convolver.Convolve(majorDimSource, minorDimSource, (x, y) => source[x, y], (x, y, v) => dest[x, y] = v);
    }
  }
}
