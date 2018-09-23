namespace VSS.TRex.SubGridTrees.Core
{
  /// <summary>
  /// A basic boolean based subgrid tree.
  /// </summary>
  public class GenericSubGridTree_Bool : GenericSubGridTree<bool, GenericLeafSubGrid_Bool>
  {
    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public GenericSubGridTree_Bool(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }
}
