using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core
{
  /// <summary>
  /// A basic int based subgrid tree.
  /// </summary>
  public class GenericSubGridTree_Int : GenericSubGridTree<int, GenericLeafSubGrid_Int>, IGenericSubGridTree_Int
  {
    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public GenericSubGridTree_Int(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }

}
