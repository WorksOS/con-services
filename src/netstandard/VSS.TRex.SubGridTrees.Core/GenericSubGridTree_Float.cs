namespace VSS.TRex.SubGridTrees.Core
{
  /// <summary>
  /// A basic float based subgrid tree.
  /// </summary>
  public class GenericSubGridTree_Float : GenericSubGridTree<float, GenericLeafSubGrid_Float>
  {
    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public GenericSubGridTree_Float(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }
}
