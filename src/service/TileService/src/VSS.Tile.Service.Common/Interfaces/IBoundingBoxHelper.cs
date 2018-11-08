using VSS.Productivity3D.Models.Models;

namespace VSS.Tile.Service.Common.Interfaces
{
  /// <summary>
  /// Bounding box helper methods.
  /// </summary>
  public interface IBoundingBoxHelper
  {   
    /// <summary>
    /// Get the bounding box values from the query parameter
    /// </summary>
    /// <param name="bbox">The query parameter containing the bounding box in decimal degrees</param>
    /// <returns>Bounding box in radians</returns>
    BoundingBox2DLatLon GetBoundingBox(string bbox);
  }
}
