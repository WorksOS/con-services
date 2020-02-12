using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.ElevationSmoothing
{
  public class SimpleAveraging_ElevationSmoother : IElevationSmootherAlgorithm
  {
    public void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf, int contextSize)
    {
      if (contextSize <= 1 || contextSize % 2 != 1)
      {
        throw new ArgumentException("Context size must be positive odd number greater than 1", nameof(contextSize));
      }

      var context = new SmootherContext<GenericLeafSubGrid_Float, float>(leaf, CellPassConsts.NullHeight);
      var contextOffset = contextSize / 2;

      // Iterate over cells in leaf and perform 3x3 averaging smoothing function on the each cell value and the surrounding values in leaf
      // ...
      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          double sum = 0;
          var numValues = 0;
          for (int x = i - contextOffset, limitx = i + contextOffset; x <= limitx; x++)
          {
            for (int y = j - contextOffset, limity = j + contextOffset; y <= limity; y++)
            {
              var value = context.Value(x, y);

              if (value != Consts.NullHeight)
              {
                sum += value;
                numValues++;
              }
            }
          }

          smoothedLeaf.Items[i, j] = numValues > 0 ? (float)(sum / numValues) : Consts.NullHeight;
        }
      }
    }


    public void SmoothArray(float[,] source, float[,] dest, int contextSize)
    {
      //      for (var i = 0, limiti = source.GetDimension(); i < SubGridTreeConsts.SubGridTreeDimension; i++)

    }
  }
}
