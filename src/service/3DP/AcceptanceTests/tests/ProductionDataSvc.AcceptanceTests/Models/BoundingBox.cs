namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// A 3D spatial extents structure
  /// </summary>
  public class BoundingBox3DGrid
  {
    public double maxX { get; set; }
    public double maxY { get; set; }
    public double maxZ { get; set; }
    public double minX { get; set; }
    public double minY { get; set; }
    public double minZ { get; set; }
  }
}
