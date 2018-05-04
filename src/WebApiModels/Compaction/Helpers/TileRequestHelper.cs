using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Helper class for constructing a tile request
  /// </summary>
  public class TileRequestHelper : DataRequestBase, ITileRequestHelper
  {
    private const int CMV_DETAILS_NUMBER_OF_COLORS = 16;
    private const int CMV_PERCENT_CHANGE_NUMBER_OF_COLORS = 9;

    private FilterResult baseFilter;
    private FilterResult topFilter;
    private VolumeCalcType? volCalcType;
    private DesignDescriptor volumeDesign;

    public TileRequestHelper SetVolumeCalcType(VolumeCalcType? calcType)
    {
      volCalcType = calcType;
      return this;
    }

    public TileRequestHelper SetVolumeDesign(DesignDescriptor volumeDesignDescriptor)
    {
      volumeDesign = volumeDesignDescriptor;
      return this;
    }

    public TileRequestHelper SetBaseFilter(FilterResult baseFilterResult)
    {
      baseFilter = baseFilterResult;
      return this;
    }

    public TileRequestHelper SetTopFilter(FilterResult topFilterResult)
    {
      topFilter = topFilterResult;
      return this;
    }

    /// <summary>
    /// Creates an instance of the TileRequest class and populate it with data needed for a tile.   
    /// </summary>
    /// <returns>An instance of the TileRequest class.</returns>
    public TileRequest CreateTileRequest(DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, ElevationStatisticsResult elevExtents)
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      var palette = SettingsManager.CompactionPalette(mode, elevExtents, ProjectSettings, ProjectSettingsColors);
      var computeVolType = (int)(volCalcType ?? VolumeCalcType.None);

      DesignDescriptor design = null;
      FilterResult filter1 = null;
      FilterResult filter2 = null;

      if (mode == DisplayMode.CutFill)
      {
        design = volCalcType == VolumeCalcType.GroundToDesign ||
                 volCalcType == VolumeCalcType.DesignToGround
          ? volumeDesign
          : DesignDescriptor;

        // For LG-D or D-LG the filter is always passed to Raptor in the Filter1 slot, Filter2 is null. 
        if (volCalcType == VolumeCalcType.DesignToGround || volCalcType == VolumeCalcType.GroundToDesign)
        {
          filter1 = baseFilter ?? topFilter;
          filter2 = null;
        }
        else
        {
          filter1 = volCalcType == VolumeCalcType.GroundToGround ||
                    volCalcType == VolumeCalcType.GroundToDesign
            ? baseFilter
            : Filter;

          // TODO (Aaron) Review, should the filter be in the Filter1 slot in this instance too, see LG-D & D-LG comment above.
          filter2 = volCalcType == VolumeCalcType.GroundToGround ||
                    volCalcType == VolumeCalcType.DesignToGround
            ? topFilter
            : null;
        }
      }

      var filterLayoutMethod = Filter == null || !Filter.LayerType.HasValue
        ? FilterLayerMethod.None
        : Filter.LayerType.Value;

      var tileRequest = TileRequest.CreateTileRequest(ProjectId, null, mode, palette, liftSettings, (RaptorConverters.VolumesType)computeVolType, 0, design, filter1, 0, filter2, 0, filterLayoutMethod, bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, CMV_PERCENT_CHANGE_NUMBER_OF_COLORS, false);

      return tileRequest;
    }
  }
}
