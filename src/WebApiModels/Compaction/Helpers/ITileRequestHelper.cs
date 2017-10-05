using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  public interface ITileRequestHelper
  {
    TileRequest CreateTileRequest(DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, ElevationStatisticsResult elevExtents);

  }
}
