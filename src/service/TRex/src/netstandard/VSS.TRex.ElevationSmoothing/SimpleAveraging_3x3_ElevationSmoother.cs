using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ElevationSmoothing
{
  public class SimpleAveraging_3x3_ElevationSmoother : IElevationSmootherAlgorithm
  {
    private const int CONTEXT_SIZE = 3;

    public void SmoothLeaf(GenericLeafSubGrid_Float leaf, GenericLeafSubGrid_Float smoothedLeaf)
    {
      var context = new SmootherContext(leaf);

      // Iterate over cells in leaf and perform 3x3 averaging smoothing function on the each cell value and the surrounding values in leaf
      // ...
      for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          double sum = 0;
          int numValues = 0;
          for (int x = i - 1; x < i + 1; x++)
          {
            for (int y = j - 1; y < j + 1; y++)
            {
              var value = context.Value(x, y);
              if (value != Consts.NullHeight)
              {
                sum += context.Value(x, y);
                numValues++;
              }
            }
          }

          smoothedLeaf.Items[i, j] = numValues > 0 ? (float)(sum / numValues) : Consts.NullHeight;
        }
      }
    }
  }
}