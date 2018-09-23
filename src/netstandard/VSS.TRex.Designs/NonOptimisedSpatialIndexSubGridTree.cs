using System.Collections.Generic;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs
{
  public class NonOptimisedSpatialIndexSubGridTree : GenericSubGridTree<List<int>, NonOptimisedSpatialIndexSubGridLeaf>
  {
    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public NonOptimisedSpatialIndexSubGridTree(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }
}
