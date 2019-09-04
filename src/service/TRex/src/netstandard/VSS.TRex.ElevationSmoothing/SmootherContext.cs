using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.ElevationSmoothing
{
  public struct SmootherContext
  {
    private const int CONTEXT_SIZE = 3;
    private const int CENTER_INDEX = 1;

    public GenericLeafSubGrid_Float[,] LeafContext;

    /// <summary>
    /// Constructs a context containing the leaf being smoothed, and the immediately surrounding leaves
    /// </summary>
    /// <param name="leaf"></param>
    public SmootherContext(GenericLeafSubGrid_Float leaf)
    {
      LeafContext = new GenericLeafSubGrid_Float[CONTEXT_SIZE, CONTEXT_SIZE];

      LeafContext[CENTER_INDEX, CENTER_INDEX] = leaf;

      for (int i = CENTER_INDEX - 1; i < CENTER_INDEX + 1; i++)
      {
        for (int j = CENTER_INDEX - 1; j < CENTER_INDEX + 1; j++)
        {
          if (LeafContext[i + 1, j + 1] != null)
            continue;

          LeafContext[i + 1, j + 1] = 
            (GenericLeafSubGrid_Float)leaf.Owner.ConstructPathToCell(leaf.OriginX + i * SubGridTreeConsts.SubGridTreeDimension, leaf.OriginY + j * SubGridTreeConsts.SubGridTreeDimension, SubGridPathConstructionType.ReturnExistingLeafOnly);
        }
      }
    }

    /// <summary>
    /// Returns the context value or the smoothing algorithm to use. This wll cross the boundaries of the sub grids in the context as required.
    /// The x and y indices are relative to the leaf being smoothed (ie: x = 0, y = 0 is the origin cell in that leaf)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public float Value(int x, int y)
    {
      var donorLeaf = LeafContext[x + SubGridTreeConsts.SubGridTreeDimension / SubGridTreeConsts.SubGridTreeDimension, 
        y + SubGridTreeConsts.SubGridTreeDimension / SubGridTreeConsts.SubGridTreeDimension];

      if (x < 0)
        x = SubGridTreeConsts.SubGridTreeDimensionMinus1 + x;
      else if (x > SubGridTreeConsts.SubGridTreeDimensionMinus1)
        x -= SubGridTreeConsts.SubGridTreeDimension;
      
      if (y < 0)
        y = SubGridTreeConsts.SubGridTreeDimensionMinus1 + y;
      else if (y > SubGridTreeConsts.SubGridTreeDimensionMinus1)
        y -= SubGridTreeConsts.SubGridTreeDimension;

      return donorLeaf?.Items[x, y] ?? Consts.NullHeight;
    }
  }
}