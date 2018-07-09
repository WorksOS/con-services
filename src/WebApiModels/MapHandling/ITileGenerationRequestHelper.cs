using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.WebApiModels.MapHandling
{
  public interface ITileGenerationRequestHelper
  {
    TileGenerationRequest CreateTileGenerationRequest(TileOverlayType[] overlays, int width, int height,
      MapType? mapType, DisplayMode? mode, string language);
  }
}
