using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
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
