using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core
{
  /// <summary>
  /// A basic long based subgrid tree.
  /// </summary>
  public class GenericSubGridTree_Long : GenericSubGridTree<long, GenericLeafSubGrid_Long>, IGenericSubGridTree_Long
  {
    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public GenericSubGridTree_Long(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }

}
