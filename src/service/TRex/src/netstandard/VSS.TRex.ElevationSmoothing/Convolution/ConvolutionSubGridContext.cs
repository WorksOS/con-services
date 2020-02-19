using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.DataSmoothing
{
  public struct ConvolutionSubGridContext<T, TV> where T : GenericLeafSubGrid<TV>
  {
    private const int SubGridContextSize = 3;
    private const int CenterIndex = SubGridContextSize / 2;

    public T[,] LeafContext;
    public readonly TV NullValue;

    /// <summary>
    /// Constructs a context containing the leaf being smoothed, and the immediately surrounding leaves
    /// </summary>
    /// <param name="leaf">The subgrid leaf to be convolved</param>
    /// <param name="nullValue">The null value to be used during convolution</param>
    public ConvolutionSubGridContext(T leaf, TV nullValue)
    {
      NullValue = nullValue;

      LeafContext = new T[SubGridContextSize, SubGridContextSize];

      LeafContext[CenterIndex, CenterIndex] = leaf;

      for (int i = CenterIndex - 1, limiti = CenterIndex + 1; i <= limiti; i++)
      {
        for (int j = CenterIndex - 1, limitj = CenterIndex + 1; j <= limitj; j++)
        {
          if (LeafContext[i, j] != null)
            continue;

          LeafContext[i, j] = leaf.Owner.ConstructPathToCell
          (leaf.OriginX + (i - 1) * SubGridTreeConsts.SubGridTreeDimension, 
            leaf.OriginY + (j - 1) * SubGridTreeConsts.SubGridTreeDimension, 
            SubGridPathConstructionType.ReturnExistingLeafOnly) as T;
        }
      }
    }

    /// <summary>
    /// Returns the context value or the smoothing algorithm to use. This will cross the boundaries of the sub grids in the context as required.
    /// The x and y indices are relative to the leaf being smoothed (ie: x = 0, y = 0 is the origin cell in that leaf)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public TV Value(int x, int y)
    {
      var donorLeaf = LeafContext[(x + SubGridTreeConsts.SubGridTreeDimension) / SubGridTreeConsts.SubGridTreeDimension,
                                    (y + SubGridTreeConsts.SubGridTreeDimension) / SubGridTreeConsts.SubGridTreeDimension];

      if (donorLeaf == null)
      {
        return NullValue;
      }

      if (x < 0)
        x += SubGridTreeConsts.SubGridTreeDimension;
      else if (x >= SubGridTreeConsts.SubGridTreeDimension)
        x -= SubGridTreeConsts.SubGridTreeDimension;

      if (y < 0)
        y += SubGridTreeConsts.SubGridTreeDimension;
      else if (y >= SubGridTreeConsts.SubGridTreeDimension)
        y -= SubGridTreeConsts.SubGridTreeDimension;

      return donorLeaf.Items[x, y];
    }
  }
}
