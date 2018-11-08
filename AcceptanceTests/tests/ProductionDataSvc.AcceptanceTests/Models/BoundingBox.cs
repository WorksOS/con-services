namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Defines a bounding box representing a 2D grid coorindate area
  /// </summary>
  public class BoundingBox2DGrid
  {
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in meters
    /// </summary>
    public double bottomLeftX { get; set; }
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in meters
    /// </summary>
    public double bottomleftY { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    public double topRightX { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    public double topRightY { get; set; }
  }

  /// <summary>
  /// Defines a bounding box representing a WGS84 latitude/longitude coorindate area
  /// </summary>
  public class BoundingBox2DLatLon
  {
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    public double bottomLeftLon { get; set; }

    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    public double bottomleftLat { get; set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    public double topRightLon { get; set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    public double topRightLat { get; set; }
  }

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
