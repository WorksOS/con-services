using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  public interface ITileRequestHelper
  {
    TileRequest CreateTileRequest(DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, ElevationStatisticsResult elevExtents, bool explicitFilters);

  }
}
