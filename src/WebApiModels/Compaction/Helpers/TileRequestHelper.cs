using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Helper class for constructing a tile request
  /// </summary>
  public class TileRequestHelper : DataRequestBase, ITileRequestHelper
  {
    private FilterResult baseFilter;
    private FilterResult topFilter;
    private VolumeCalcType? volCalcType;
    private DesignDescriptor volumeDesign;

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

    public TileRequestHelper SetVolumeCalcType(VolumeCalcType? calcType)
    {
      this.volCalcType = calcType;
      return this;
    }

    public TileRequestHelper SetVolumeDesign(DesignDescriptor volumeDesign)
    {
      this.volumeDesign = volumeDesign;
      return this;
    }

    public TileRequestHelper SetBaseFilter(FilterResult baseFilter)
    {
      this.baseFilter = baseFilter;
      return this;
    }

    public TileRequestHelper SetTopFilter(FilterResult topFilter)
    {
      this.topFilter = topFilter;
      return this;
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
      var palette = SettingsManager.CompactionPalette(mode, elevExtents, ProjectSettings, ProjectSettingsColors);
      var computeVolType = (int)(volCalcType ?? VolumeCalcType.None);

      DesignDescriptor design = DesignDescriptor;
      FilterResult filter1 = Filter;
      FilterResult filter2 = null;

      if (mode == DisplayMode.CutFill)
      {
        switch (volCalcType)
        {
          case VolumeCalcType.DesignToGround:
          case VolumeCalcType.GroundToDesign:
            design = volumeDesign;
            filter1 = baseFilter ?? topFilter;
            break;
          case VolumeCalcType.GroundToGround:
            filter1 = baseFilter;
            filter2 = topFilter;
            break;
        }
      }

      TileRequest tileRequest = TileRequest.CreateTileRequest(
        ProjectId, null, mode, palette, liftSettings, (VolumesType)computeVolType,
        0, design, filter1, 0, filter2, 0,
        Filter == null || !Filter.LayerType.HasValue ? FilterLayerMethod.None : Filter.LayerType.Value,
        bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, CMV_PERCENT_CHANGE_NUMBER_OF_COLORS, false);

      return tileRequest;
    }

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 5;
    private const int CMV_PERCENT_CHANGE_NUMBER_OF_COLORS = 9;
  }
}
