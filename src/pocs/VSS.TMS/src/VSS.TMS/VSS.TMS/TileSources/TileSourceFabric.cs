
using VSS.TMS.TileSources;

namespace VSS.TMS.Controllers
{
  class TileSourceFabric
  {
    public static ITileSource CreateTileSource(TileSetConfiguration configuration)
    {
      if (Utils.IsLocalFileScheme(configuration.Source))
      {
        return new LocalTileSource(configuration);
      }
      else
      {
        return null;
      }
    }
  }
}
