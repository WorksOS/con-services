using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Models
{
  public class CompactionTileV2Request : TileRequest
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionTileV2Request()
    { }

    /// <summary>
    /// Create instance of TileRequest
    /// </summary>
    public static CompactionTileV2Request CreateCompactionRaptorTileRequest(
        long projectId,
        Guid? callId,
        DisplayMode mode,
        List<ColorPalette> palettes,
        LiftBuildSettings liftBuildSettings,
        RaptorConverters.VolumesType computeVolType,
        double computeVolNoChangeTolerance,
        DesignDescriptor designDescriptor,
        Filter filter1,
        long filterId1,
        Filter filter2,
        long filterId2,
        FilterLayerMethod filterLayerMethod,
        BoundingBox2DLatLon boundingBoxLatLon,
        BoundingBox2DGrid boundingBoxGrid,
        ushort width,
        ushort height,
        uint representationalDisplayColor = 0
      )
    {
      return new CompactionTileV2Request
      {
        projectId = projectId,
        callId = callId,
        mode = mode,
        palettes = palettes,
        liftBuildSettings = liftBuildSettings,
        computeVolType = computeVolType,
        computeVolNoChangeTolerance = computeVolNoChangeTolerance,
        designDescriptor = designDescriptor,
        filter1 = filter1,
        filterId1 = filterId1,
        filter2 = filter2,
        filterId2 = filterId2,
        filterLayerMethod = filterLayerMethod,
        boundBoxLL = boundingBoxLatLon,
        boundBoxGrid = boundingBoxGrid,
        width = width,
        height = height,
        representationalDisplayColor = representationalDisplayColor
      };
    }
    protected override int cmvDetailsColorNumber => CMV_DETAILS_COLOR_NUMBER;
    public override bool setSummaryDataLayersVisibility => true;

    private const int CMV_DETAILS_COLOR_NUMBER = 11;
  }
}
