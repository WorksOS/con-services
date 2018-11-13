using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs
{
  public class OptimisedSpatialIndexSubGridTree : GenericSubGridTree<TriangleArrayReference, TriangleArrayReferenceSubGrid>
  {
    /// <summary>
    /// The header string to be written into a serialized subgrid tree
    /// </summary>
    /// <returns></returns>
    public override string SerialisedHeaderName() => "OptmisedSpatialIndex";

    /// <summary>
    /// The header version to be written into a serialized subgrid tree
    /// </summary>
    /// <returns></returns>
    public override int SerialisedVersion() => 1;

    /// <summary>
    /// Generic sub grid tree constructor. Accepts standard cell size and number of levels
    /// </summary>
    /// <param name="numLevels"></param>
    /// <param name="cellSize"></param>
    public OptimisedSpatialIndexSubGridTree(byte numLevels, double cellSize) : base(numLevels, cellSize)
    { }
  }
}
