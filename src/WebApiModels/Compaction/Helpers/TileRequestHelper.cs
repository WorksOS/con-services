using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Helper class for constructing a tile request
  /// </summary>
  public class TileRequestHelper : DataRequestBase, ITileRequestHelper
  {
    public TileRequestHelper()
    { }

    public TileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the TileRequest class and populate it with data needed for a tile.   
    /// </summary>
    /// <returns>An instance of the TileRequest class.</returns>
    public TileRequest CreateTileRequest(DisplayMode mode, ushort width, ushort height,
      BoundingBox2DLatLon bbox, ElevationStatisticsResult elevExtents)
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      Filter?.Validate();//Why is this here? Should be done where filter set up???
      var palette = SettingsManager.CompactionPalette(mode, elevExtents, ProjectSettings);

      TileRequest tileRequest = TileRequest.CreateTileRequest(
        ProjectId, null, mode, palette, liftSettings, RaptorConverters.VolumesType.None, 
        0, DesignDescriptor, Filter, 0, null, 0,
        Filter == null || !Filter.layerType.HasValue ? FilterLayerMethod.None : Filter.layerType.Value,
        bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, false);

      return tileRequest;
    }

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 16;
  }
}