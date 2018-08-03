using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApiModels.MapHandling
{
  public interface ITileGenerationRequestHelper
  {
    TileGenerationRequest CreateTileGenerationRequest(TileOverlayType[] overlays, int width, int height,
      MapType? mapType, DisplayMode? mode, string language);
  }
}
