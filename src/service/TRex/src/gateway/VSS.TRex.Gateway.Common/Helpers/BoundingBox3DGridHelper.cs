using VSS.MasterData.Models.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public static class BoundingBox3DGridHelper
  {
    /// <summary>
    /// Converts BoundingWorldExtent3D data into BoundingBox3DGrid data.
    /// </summary>
    public static BoundingBox3DGrid ConvertExtents(BoundingWorldExtent3D extents)
    {
      return new BoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ);
    }
  }
}
