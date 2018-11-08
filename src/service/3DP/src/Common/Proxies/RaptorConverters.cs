using System;
using System.Collections.Generic;
using System.Linq;
using BoundingExtents;
using Fences;
using Microsoft.Extensions.Logging;
using SubGridTreesDecls;
using SVOICDecls;
using SVOICFiltersDecls;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using SVOSiteVisionDecls;
using VLPDDecls;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using __Global = ProductionServer_TLB.__Global;
using Point = VSS.MasterData.Models.Models.Point;
using WGSPoint = VSS.Productivity3D.Models.Models.WGSPoint3D;

namespace VSS.Productivity3D.Common.Proxies
{
  //TODO simplify and refactor this ugly class

  public static class RaptorConverters
  {
    public static readonly DateTime PDS_MIN_DATE = new DateTime(1899, 12, 30, 0, 0, 0);

    public static IEnumerable<WGSPoint> geometryToPoints(string geometry)
    {
      const double DEGREES_TO_RADIANS = Math.PI / 180;

      List<WGSPoint> latlngs = new List<WGSPoint>();
      //Trim off the "POLYGON((" and "))"
      geometry = geometry.Substring(9, geometry.Length - 11);
      var points = geometry.Split(',');
      foreach (var point in points)
      {
        var parts = point.Trim().Split(' ');
        var lng = double.Parse(parts[0]);
        var lat = double.Parse(parts[1]);
        latlngs.Add(new WGSPoint(lat * DEGREES_TO_RADIANS, lng * DEGREES_TO_RADIANS));
      }
      return latlngs;
    }

    public static void AdjustFilterToFilter(ref TICFilterSettings baseFilter, TICFilterSettings topFilter)
    {
      //Special case for Raptor filter to filter comparisons.
      //If base filter is earliest and top filter is latest with a DateTime filter then replace
      //base filter with latest with a date filter with the start date at the beginning of time and 
      //the end date at the original start date. This is to avoid losing data between original start date
      //and first event after the start date with data.
      if (baseFilter.HasTimeComponent() && baseFilter.ReturnEarliestFilteredCellPass &&
          topFilter.HasTimeComponent() && !topFilter.ReturnEarliestFilteredCellPass)
      {
        topFilter.SetElevationTypeCellpassState(false);

        baseFilter = AdjustBaseFilter(baseFilter);
      }
    }

    /// <summary>
    /// Returns adjusted filter settings copy for case of cached filter.
    /// </summary>
    public static TICFilterSettings AdjustBaseFilter(TICFilterSettings baseFilter)
    {
      var copy = new TICFilterSettings();
      copy.Assign(baseFilter);
      copy.EndTime = baseFilter.StartTime;
      copy.StartTime = PDS_MIN_DATE;
      copy.ReturnEarliestFilteredCellPass = false;
      copy.ElevationType = TICElevationType.etLast;
      copy.SetElevationTypeCellpassState(false);

      return copy;
    }

    public static TColourPalettes convertColorPalettes(List<ColorPalette> palettes, DisplayMode mode)
    {
      TColourPalettes result = new TColourPalettes();

      if ((mode == DisplayMode.CCA) || (mode == DisplayMode.CCASummary))
        return result;

      if (palettes == null || palettes.Count == 0)
        palettes = defaultColorPalette(mode);

      if (palettes.Count > 0)
      {
        result.Transitions = new TColourPalette[palettes.Count];

        for (int i = 0; i < palettes.Count; i++)
        {
          result.Transitions[i].Colour = palettes[i].Color;
          result.Transitions[i].Value = palettes[i].Value;
        }
      }

      return result;
    }


    // RGBToColor comes from WebMapCache.cs
    private static int RGBToColor(int r, int g, int b)
    {
      return r << 16 | g << 8 | b << 0;
    }

    // ElevationPalette comes from WebMapCache.cs
    public static List<int> ElevationPalette()
    {
      return new List<int> {
              RGBToColor(255,0,0),
              RGBToColor(225,60,0),
              RGBToColor(255,90,0),
              RGBToColor(255,130,0),
              RGBToColor(255,170,0),
              RGBToColor(255,200,0),
              RGBToColor(255,220,0),
              RGBToColor(250,230,0),
              RGBToColor(220,230,0),
              RGBToColor(210,230,0),
              RGBToColor(200,230,0),
              RGBToColor(180,230,0),
              RGBToColor(150,230,0),
              RGBToColor(130,230,0),
              RGBToColor(100,240,0),
              RGBToColor(0,255,0),
              RGBToColor(0,240,100),
              RGBToColor(0,230,130),
              RGBToColor(0,230,150),
              RGBToColor(0,230,180),
              RGBToColor(0,230,200),
              RGBToColor(0,230,210),
              RGBToColor(0,220,220),
              RGBToColor(0,200,230),
              RGBToColor(0,180,240),
              RGBToColor(0,150,245),
              RGBToColor(0,120,250),
              RGBToColor(0,90,255),
              RGBToColor(0,70,255),
              RGBToColor(0,0,255)
            };
    }

    private static List<ColorPalette> defaultColorPalette(DisplayMode mode)
    {
      ColorSettings cs = ColorSettings.Default;
      List<ColorPalette> palettes = null;
      switch (mode)
      {
        case DisplayMode.Design3D:
        case DisplayMode.Height:
          int numberOfColors = 30;

          double step = (cs.elevationMaximum.value - cs.elevationMinimum.value) / (numberOfColors - 1);
          List<int> colors = ElevationPalette();

          List<ColorPalette> paletteList = new List<ColorPalette>();
          paletteList.Add(new ColorPalette(cs.elevationBelowColor, -1));
          for (int i = 0; i < colors.Count; i++)
          {
            paletteList.Add(new ColorPalette((uint)colors[i], cs.elevationMinimum.value + i * step));
          }
          paletteList.Add(new ColorPalette(cs.elevationAboveColor, -1));

          palettes = paletteList;
          break;

        case DisplayMode.CCV:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.cmvMinimum.color, cs.cmvMinimum.value ),
            new ColorPalette(cs.cmvTarget.color, 0.9 * cs.cmvTarget.value ),
            new ColorPalette(cs.cmvTarget.color, 1.1 * cs.cmvTarget.value ),
            new ColorPalette(cs.cmvMaximum.color, cs.cmvMaximum.value ) };
          break;

        case DisplayMode.CCVPercentChange:

        case DisplayMode.CCVPercent:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.cmvPercentMinimum.color, cs.cmvPercentMinimum.value ),
            new ColorPalette(cs.cmvPercentTarget.color, 0.9 * cs.cmvPercentTarget.value ),
            new ColorPalette(cs.cmvPercentTarget.color, 1.1 * cs.cmvPercentTarget.value ),
            new ColorPalette(cs.cmvPercentMaximum.color, cs.cmvPercentMaximum.value ) };
          break;

        case DisplayMode.CMVChange:
          palettes = new List<ColorPalette> {
            new ColorPalette(0, 0  ),
            new ColorPalette(65280, 10),
            new ColorPalette(16776960, 20 ),
            new ColorPalette(16744192, 40 ),
            new ColorPalette(16711935, 80 ),
            new ColorPalette(16711680, double.MaxValue )
                    };
          break;

        case DisplayMode.Latency:
          break;

        case DisplayMode.PassCount:
          palettes = new List<ColorPalette>();

          for (int i = cs.passCountDetailColors.Count - 1; i >= 0; i--)
            palettes.Insert(cs.passCountDetailColors.Count - i - 1, new ColorPalette(cs.passCountDetailColors[i].color, cs.passCountDetailColors[i].value));
          break;

        case DisplayMode.PassCountSummary:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.passCountMinimum.color, cs.passCountMinimum.value ),
            new ColorPalette(cs.passCountTarget .color, cs.passCountTarget .value ),
            new ColorPalette(cs.passCountMaximum.color, cs.passCountMaximum.value ) };
          break;

        case DisplayMode.RMV:
          break;
        case DisplayMode.Frequency:
          break;
        case DisplayMode.Amplitude:
          break;

        case DisplayMode.CutFill:
          // TODO This needs to be completed to define the sets of cut/fill shells defined in the settings.
          palettes = new List<ColorPalette>();
          for (int i = 0; i < cs.cutFillColors.Count; i++)
            palettes.Add(new ColorPalette(cs.cutFillColors[i].color, cs.cutFillColors[i].value));
          break;

        case DisplayMode.Moisture:
          break;
        case DisplayMode.TemperatureSummary:
          // ajr14976

          palettes = new List<ColorPalette> {
            new ColorPalette(cs.temperatureMinimumColor, 0 ),
            new ColorPalette(cs.temperatureTargetColor, 1 ),
            new ColorPalette(cs.temperatureMaximumColor, 2 ) };

          break;
        case DisplayMode.GPSMode:
          break;
        case DisplayMode.CCVSummary:
        case DisplayMode.CCVPercentSummary:
          // Hard code the summary Colors into a transitions palette for now to push it through the current pallete transfer machanism in 
          // the tile requests. The tile processor will unpack it into an appropriate structure on the Raptor side.
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.ccvSummaryCompleteLayerColor, 0 ),
            new ColorPalette(cs.ccvSummaryWorkInProgressLayerColor, 1 ),
            new ColorPalette(cs.ccvSummaryUndercompactedLayerColor, 2 ),
            new ColorPalette(cs.ccvSummaryOvercompactedLayerColor, 3 ),
            new ColorPalette(cs.ccvSummaryTooThickLayerColor, 4 ),
            new ColorPalette(cs.ccvSummaryApprovedLayerColor, 5 )};
          break;
        case DisplayMode.CompactionCoverage:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.coverageColor, 0 ),
            new ColorPalette(cs.surveyedSurfaceColor, 1 ) };
          break;
        case DisplayMode.TargetThicknessSummary:
        case DisplayMode.VolumeCoverage:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.volumeSummaryCoverageColor, 0 ),
            new ColorPalette(cs.volumeSummaryVolumeColor, 1 ),
            new ColorPalette(cs.volumeSummaryNoChangeColor, 2 ) };
          break;
        case DisplayMode.MDP:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.mdpMinimum.color, cs.mdpMinimum.value ),
            new ColorPalette(cs.mdpTarget.color, 0.9 * cs.mdpTarget.value ),
            new ColorPalette(cs.mdpTarget.color, 1.1 * cs.mdpTarget.value ),
            new ColorPalette(cs.mdpMaximum.color, cs.mdpMaximum.value ) };
          break;
        case DisplayMode.MDPPercent:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.mdpPercentMinimum.color, cs.mdpPercentMinimum.value ),
            new ColorPalette(cs.mdpPercentTarget.color, 0.9 * cs.mdpPercentTarget.value ),
            new ColorPalette(cs.mdpPercentTarget.color, 1.1 * cs.mdpPercentTarget.value ),
            new ColorPalette(cs.mdpPercentMaximum.color, cs.mdpPercentMaximum.value ) };
          break;
        case DisplayMode.MDPSummary:
        case DisplayMode.MDPPercentSummary:
          // Hard code the summary Colors into a transitions palette for now to push it through the current pallete transfer machanism in 
          // the tile requests. The tile processor will unpack it into an appropriate structure on the Raptor side.
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.mdpSummaryCompleteLayerColor, 0 ),
            new ColorPalette(cs.mdpSummaryWorkInProgressLayerColor, 1 ),
            new ColorPalette(cs.mdpSummaryUndercompactedLayerColor, 2 ),
            new ColorPalette(cs.mdpSummaryOvercompactedLayerColor, 3 ),
            new ColorPalette(cs.mdpSummaryTooThickLayerColor, 4 ),
            new ColorPalette(cs.mdpSummaryApprovedLayerColor, 5 )};
          break;
        case DisplayMode.MachineSpeed:
          palettes = new List<ColorPalette>();

          for (int i = cs.machineSpeedColors.Count - 1; i >= 0; i--)
            palettes.Insert(cs.machineSpeedColors.Count - i - 1, new ColorPalette(cs.machineSpeedColors[i].color, cs.machineSpeedColors[i].value));
          break;
        case DisplayMode.TargetSpeedSummary:
          palettes = new List<ColorPalette> {
            new ColorPalette(cs.machineSpeedMinimumColor, 0 ),
            new ColorPalette(cs.machineSpeedTargetColor, 1 ),
            new ColorPalette(cs.machineSpeedMaximumColor, 2 ) };
          break;
        case DisplayMode.TemperatureDetail:
          palettes = new List<ColorPalette> {
            new ColorPalette(0x01579B, 0 ),
            new ColorPalette(0x039BE5, 500 ),
            new ColorPalette(0xB3E5FC, 1000 ),
            new ColorPalette(0x8BC34A, 1500 ),
            new ColorPalette(0xFFCDD2, 2000 ),
            new ColorPalette(0xE57373, 2500 ),
            new ColorPalette(0xD50000, 3000 ) };
          break;
      }

      return palettes;
    }


    public static IEnumerable<MachineDetails> converMachineDetails(TMachineDetail[] machineIDs)
    {
      foreach (TMachineDetail machineDetail in machineIDs)
        yield return
            MachineDetails.Create(machineDetail.ID, machineDetail.Name, machineDetail.IsJohnDoeMachine);
    }

    public static BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(

              extents.MinX,
              extents.MinY,
              extents.MinZ,
              extents.MaxX,
              extents.MaxY,
              extents.MaxZ
              );
    }

    public static DisplayMode convertDisplayMode(TICDisplayMode mode)
    {
      switch (mode)
      {
        case TICDisplayMode.icdmHeight: return DisplayMode.Height;
        case TICDisplayMode.icdmCCV: return DisplayMode.CCV;
        case TICDisplayMode.icdmCCVPercent: return DisplayMode.CCVPercent;
        case TICDisplayMode.icdmLatency: return DisplayMode.Latency;
        case TICDisplayMode.icdmPassCount: return DisplayMode.PassCount;
        case TICDisplayMode.icdmRMV: return DisplayMode.RMV;
        case TICDisplayMode.icdmFrequency: return DisplayMode.Frequency;
        case TICDisplayMode.icdmAmplitude: return DisplayMode.Amplitude;
        case TICDisplayMode.icdmCutFill: return DisplayMode.CutFill;
        case TICDisplayMode.icdmMoisture: return DisplayMode.Moisture;
        case TICDisplayMode.icdmTemperatureSummary: return DisplayMode.TemperatureSummary;
        case TICDisplayMode.icdmGPSMode: return DisplayMode.GPSMode;
        case TICDisplayMode.icdmCCVSummary: return DisplayMode.CCVSummary;
        case TICDisplayMode.icdmCCVPercentSummary: return DisplayMode.CCVPercentSummary;   // This is a synthetic display mode for CCV summary
        case TICDisplayMode.icdmPassCountSummary: return DisplayMode.PassCountSummary;    // This is a synthetic display mode for Pass Count summary
        case TICDisplayMode.icdmCompactionCoverage: return DisplayMode.CompactionCoverage;  // This ia a synthetic display mode for Compaction Coverage
        case TICDisplayMode.icdmVolumeCoverage: return DisplayMode.VolumeCoverage;      // This is a synthetic display mode for Volumes Coverage
        case TICDisplayMode.icdmMDP: return DisplayMode.MDP;
        case TICDisplayMode.icdmMDPSummary: return DisplayMode.MDPSummary;
        case TICDisplayMode.icdmMDPPercent: return DisplayMode.MDPPercent;
        case TICDisplayMode.icdmMDPPercentSummary: return DisplayMode.MDPPercentSummary;   // This is a synthetic display mode for MDP summary
        case TICDisplayMode.icdmCellProfile: return DisplayMode.CellProfile;
        case TICDisplayMode.icdmCellPasses: return DisplayMode.CellPasses;
        case TICDisplayMode.icdmMachineSpeed: return DisplayMode.MachineSpeed;
        case TICDisplayMode.icdmCCVPercentChange: return DisplayMode.CCVPercentChange;
        case TICDisplayMode.icdmTargetThicknessSummary: return DisplayMode.TargetThicknessSummary;
        case TICDisplayMode.icdmTargetSpeedSummary: return DisplayMode.TargetSpeedSummary;
        case TICDisplayMode.icdmCCVChange: return DisplayMode.CMVChange;
        case TICDisplayMode.icdmCCA: return DisplayMode.CCA;
        case TICDisplayMode.icdmCCASummary: return DisplayMode.CCASummary;
        case TICDisplayMode.icdmTemperatureDetail: return DisplayMode.TemperatureDetail;
        case TICDisplayMode.icdm3DTerrain: return DisplayMode.Terrain3D;
        case TICDisplayMode.icdm3DDesign: return DisplayMode.Design3D;
        default: throw new Exception($"Unknown TICDisplayMode {Convert.ToInt16(mode)}");
      }
    }

    public static TICDisplayMode convertDisplayMode(DisplayMode mode)
    {
      switch (mode)
      {
        case DisplayMode.Height: return TICDisplayMode.icdmHeight;
        case DisplayMode.CCV: return TICDisplayMode.icdmCCV;
        case DisplayMode.CCVPercent: return TICDisplayMode.icdmCCVPercent;
        case DisplayMode.Latency: return TICDisplayMode.icdmLatency;
        case DisplayMode.PassCount: return TICDisplayMode.icdmPassCount;
        case DisplayMode.RMV: return TICDisplayMode.icdmRMV;
        case DisplayMode.Frequency: return TICDisplayMode.icdmFrequency;
        case DisplayMode.Amplitude: return TICDisplayMode.icdmAmplitude;
        case DisplayMode.CutFill: return TICDisplayMode.icdmCutFill;
        case DisplayMode.Moisture: return TICDisplayMode.icdmMoisture;
        case DisplayMode.TemperatureSummary: return TICDisplayMode.icdmTemperatureSummary;
        case DisplayMode.GPSMode: return TICDisplayMode.icdmGPSMode;
        case DisplayMode.CCVSummary: return TICDisplayMode.icdmCCVSummary;
        case DisplayMode.CCVPercentSummary: return TICDisplayMode.icdmCCVPercentSummary;   // This is a synthetic display mode for CCV summary
        case DisplayMode.PassCountSummary: return TICDisplayMode.icdmPassCountSummary;    // This is a synthetic display mode for Pass Count summary
        case DisplayMode.CompactionCoverage: return TICDisplayMode.icdmCompactionCoverage;  // This ia a synthetic display mode for Compaction Coverage
        case DisplayMode.VolumeCoverage: return TICDisplayMode.icdmVolumeCoverage;      // This is a synthetic display mode for Volumes Coverage
        case DisplayMode.MDP: return TICDisplayMode.icdmMDP;
        case DisplayMode.MDPSummary: return TICDisplayMode.icdmMDPSummary;
        case DisplayMode.MDPPercent: return TICDisplayMode.icdmMDPPercent;
        case DisplayMode.MDPPercentSummary: return TICDisplayMode.icdmMDPPercentSummary;   // This is a synthetic display mode for MDP summary
        case DisplayMode.CellProfile: return TICDisplayMode.icdmCellProfile;
        case DisplayMode.CellPasses: return TICDisplayMode.icdmCellPasses;
        case DisplayMode.MachineSpeed: return TICDisplayMode.icdmMachineSpeed;
        case DisplayMode.CCVPercentChange: return TICDisplayMode.icdmCCVPercentChange;
        case DisplayMode.TargetThicknessSummary: return TICDisplayMode.icdmTargetThicknessSummary;
        case DisplayMode.TargetSpeedSummary: return TICDisplayMode.icdmTargetSpeedSummary;
        case DisplayMode.CMVChange: return TICDisplayMode.icdmCCVChange;
        case DisplayMode.CCA: return TICDisplayMode.icdmCCA;
        case DisplayMode.CCASummary: return TICDisplayMode.icdmCCASummary;
        case DisplayMode.TemperatureDetail: return TICDisplayMode.icdmTemperatureDetail;
        case DisplayMode.Terrain3D: return TICDisplayMode.icdm3DTerrain;
        case DisplayMode.Design3D: return TICDisplayMode.icdm3DDesign;
        default: throw new Exception($"Unknown DisplayMode {Convert.ToInt16(mode)}");
      }
    }


    public static TWGS84Point convertWGSPoint(WGSPoint point)
    {
      return new TWGS84Point
      {
        Lat = point.Lat,
        Lon = point.Lon
      };
    }

    /// <summary>
    /// Container to fence
    /// </summary>
    /// <param name="fence"></param>
    /// <returns></returns>
    public static TWGS84FenceContainer convertWGS84Fence(WGS84Fence fence)
    {
      TWGS84FenceContainer fenceContainer = new TWGS84FenceContainer
      {
        FencePoints = fence.Points.ToList().ConvertAll(p => new TWGS84Point { Lat = p.Lat, Lon = p.Lon }).ToArray()
      };

      return fenceContainer;
    }

    public static TICFilterSettings ConvertFilter(long? filterID, FilterResult filter, long? projectid, DateTime? overrideStartUTC = null,
        DateTime? overrideEndUTC = null, List<long> overrideAssetIds = null, string fileSpaceName = null, ILogger log = null)
    {
      if (filter != null)
      {
        return ConvertFilter(filter, overrideStartUTC, overrideEndUTC, overrideAssetIds, fileSpaceName, log);
      }

      if (filterID > 0)
      {
        throw new NotImplementedException("Filter service not implemented yet");
        /* COMMENT OUT UNTIL FILTER SVC BUILT 
          //Get filter from Filter Service
          return ConvertFilter(ServiceLocator.GetFiltersSvc().GetFilters(projectid ?? -1, filterID.Value).FiltersArray.FirstOrDefault(), overrideStartUTC,
              overrideEndUTC, overrideAssetIds);
              */
      }

      // No filter specified, use default
      return DefaultRaptorFilter;
    }

    private static TICFilterSettings DefaultRaptorFilter => new TICFilterSettings { LayerMethod = TFilterLayerMethod.flmAutoMapReset };

    //TODO split this method
    //TODO think that this method coul be common as will be consumed by others
    //TODO test this
    public static TICFilterSettings ConvertFilter(FilterResult pdf, DateTime? overrideStartUTC = null, DateTime? overrideEndUTC = null, List<long> overrideAssetIds = null, string fileSpaceName = null, ILogger log = null)
    {
      const double RADIANS_TO_DEGREES = 180.0 / Math.PI;

      TICFilterSettings filter = DefaultRaptorFilter;
      List<TMachineDetail> assetList = null;

      if (overrideStartUTC.HasValue)
      {
        filter.StartTime = overrideStartUTC.Value;
        filter.SetTimeCellpassState(true);
      }
      else if (pdf != null && pdf.StartUtc.HasValue)
      {
        filter.StartTime = pdf.StartUtc.Value;
        filter.SetTimeCellpassState(true);
      }

      if (overrideEndUTC.HasValue)
      {
        filter.EndTime = overrideEndUTC.Value;
        filter.SetTimeCellpassState(true);
      }
      else if (pdf != null && pdf.EndUtc.HasValue)
      {
        filter.EndTime = pdf.EndUtc.Value;
        filter.SetTimeCellpassState(true);
      }

      if (overrideAssetIds != null && overrideAssetIds.Count > 0 && pdf == null)
      {
        if (assetList == null)
        {
          assetList = (from a in overrideAssetIds select new TMachineDetail { Name = string.Empty, ID = a, IsJohnDoeMachine = false }).ToList();
        }
      }

      if (pdf != null)
      {

        // Currently the Raptor code only supports filtering on a single Machine Design
        if (pdf.OnMachineDesignId.HasValue)
        {
          filter.DesignNameID = (int)pdf.OnMachineDesignId.Value;
          filter.SetDesignNameCellpassState(true);
        }


        if (pdf.AssetIDs != null && pdf.AssetIDs.Count > 0)
        {
          assetList = (from a in pdf.AssetIDs select new TMachineDetail { Name = string.Empty, ID = a, IsJohnDoeMachine = false }).ToList();
        }

        List<TMachineDetail> machineList = null;
        if (pdf.ContributingMachines != null && pdf.ContributingMachines.Count > 0)
        {
          machineList = (from c in pdf.ContributingMachines select new TMachineDetail { Name = c.MachineName, ID = c.AssetId, IsJohnDoeMachine = c.IsJohnDoe }).ToList();
          if (assetList == null)
            assetList = machineList;
          else
            assetList.AddRange(machineList);
        }

        if (overrideAssetIds != null && overrideAssetIds.Count > 0)
        {
          if (assetList == null)
          {
            assetList = (from a in overrideAssetIds select new TMachineDetail { Name = string.Empty, ID = a, IsJohnDoeMachine = false }).ToList();
          }
          else
          {
            //Both project filter and report have assets selected so use intersection
            assetList = (from a in assetList where overrideAssetIds.Contains(a.ID) select a).ToList();
          }
        }

        if (pdf.CompactorDataOnly.HasValue)
        {
          filter.SetCompactionMachinesOnlyState(pdf.CompactorDataOnly.Value);
        }

        if (pdf.VibeStateOn.HasValue)
        {
          filter.VibeState = pdf.VibeStateOn.Value ? TICVibrationState.vsOn : TICVibrationState.vsOff;
          filter.SetVibeStateCellpassState(true);
        }

        if (pdf.ElevationType.HasValue)
        {
          filter.ElevationType = ConvertElevationType(pdf.ElevationType.Value);
          filter.SetElevationTypeCellpassState(true);
        }

        //Note: the SiteID is only used for the UI. The points of the site or user-defined polygon are in Polygon.
        if (pdf.PolygonLL != null && pdf.PolygonLL.Count > 0)
        {
          //NOTE: There is an inconsistency inherited from VL where the filter is passed to Raptor with decimal degrees.
          //All other lat/lngs in Shim calls are passed to Raptor as radians. Since we now have consistency in the Raptor
          //services where everything is radians we need to convert to decimal degrees here for the filter to match VL.
          foreach (WGSPoint p in pdf.PolygonLL)
          {
            filter.Fence.Add(new TFencePoint(p.Lon * RADIANS_TO_DEGREES, p.Lat * RADIANS_TO_DEGREES, 0));
          }

          filter.SetPositionalCellSpatialSelectionState(true);
          filter.CoordsAreGrid = false;
        }
        else
            if (pdf.PolygonGrid != null && pdf.PolygonGrid.Count > 0)
        {
          foreach (Point p in pdf.PolygonGrid)
          {
            filter.Fence.Add(new TFencePoint(p.x, p.y, 0));
          }

          filter.SetPositionalCellSpatialSelectionState(true);
          filter.CoordsAreGrid = true;
        }


        if (pdf.ForwardDirection.HasValue)
        {
          filter.MachineDirection = pdf.ForwardDirection.Value ? TICMachineDirection.mdForward : TICMachineDirection.mdReverse;
          filter.SetMachineDirectionCellpassState(true);
        }

        if (pdf.AlignmentFile != null && pdf.StartStation.HasValue && pdf.EndStation.HasValue && pdf.LeftOffset.HasValue && pdf.RightOffset.HasValue)
        {
          filter.ReferenceDesign = DesignDescriptor(pdf.AlignmentFile);
          filter.StartStation = pdf.StartStation.Value;
          filter.EndStation = pdf.EndStation.Value;
          filter.LeftOffset = pdf.LeftOffset.Value;
          filter.RightOffset = pdf.RightOffset.Value;

          filter.SetDesignMaskCellSelectionState(true);
        }

        // Layer Analysis
        if (pdf.LayerType.HasValue)
        {
          filter.LayerMethod = ConvertLayerMethod(pdf.LayerType.Value);
          filter.LayerState = TICLayerState.lsOn;

          if (filter.LayerMethod == TFilterLayerMethod.flmOffsetFromDesign || filter.LayerMethod == TFilterLayerMethod.flmOffsetFromBench || filter.LayerMethod == TFilterLayerMethod.flmOffsetFromProfile)
          {
            if (filter.LayerMethod == TFilterLayerMethod.flmOffsetFromBench)
            {
              filter.ElevationRangeLevel = pdf.BenchElevation.HasValue ? pdf.BenchElevation.Value : 0;
            }
            else
            {
              filter.ElevationRangeDesign = DesignDescriptor(pdf.LayerDesignOrAlignmentFile);
            }
            if (pdf.LayerNumber.HasValue && pdf.LayerThickness.HasValue)
            {
              int layerNumber = pdf.LayerNumber.Value < 0 ? pdf.LayerNumber.Value + 1 : pdf.LayerNumber.Value;
              filter.ElevationRangeOffset = layerNumber * pdf.LayerThickness.Value;
              filter.ElevationRangeThickness = pdf.LayerThickness.Value;
            }
            else
            {
              filter.ElevationRangeOffset = 0;
              filter.ElevationRangeThickness = 0;
            }
            filter.SetElevationRangeCellPassState(true);
          }
          else if (filter.LayerMethod == TFilterLayerMethod.flmTagfileLayerNumber)
          {
            filter.LayerID = pdf.LayerNumber.Value;
            filter.PassFilterSelections = filter.PassFilterSelections.Set(TICFilterPassSelection.icfsLayerID);

          }
        }
        else
          filter.LayerState = TICLayerState.lsOff;

        if (pdf.GpsAccuracy.HasValue)
        {
          //TODO Do safe casting here
          filter.GPSAccuracy = ((TICGPSAccuracy)pdf.GpsAccuracy);
          filter.GPSAccuracyIsInclusive = pdf.GpsAccuracyIsInclusive ?? false;
          filter.PassFilterSelections = filter.PassFilterSelections.Set(TICFilterPassSelection.icfsGPSAccuracy);
        }


        if (pdf.BladeOnGround.HasValue && pdf.BladeOnGround.Value)
        {
          filter.SetPassTypeState(true);
          filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptFront);
          filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptRear);
        }
        if (pdf.TrackMapping.HasValue && pdf.TrackMapping.Value)
        {
          filter.SetPassTypeState(true);
          filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptTrack);
        }
        if (pdf.WheelTracking.HasValue && pdf.WheelTracking.Value)
        {
          filter.SetPassTypeState(true);
          filter.PassTypeSelections = filter.PassTypeSelections.Set(TICPassType.ptWheel);
        }

        if (pdf.DesignFile != null)
        {
          filter.DesignFilter = VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(
            pdf.DesignFile.Id,
            fileSpaceName,
            pdf.DesignFile.File.filespaceId,
            pdf.DesignFile.File.path,
            pdf.DesignFile.File.fileName,
            pdf.DesignFile.Offset);

          filter.SetDesignFilterMaskCellSelectionState(true);
        }
      }

      if ((pdf != null) && (pdf.SurveyedSurfaceExclusionList != null))
      {
        filter.SurveyedSurfaceExclusionList = (from a in pdf.SurveyedSurfaceExclusionList select new TSurveyedSurfaceID { SurveyedSurfaceID = a }).ToArray();
      }

      if (assetList != null)
      {
        filter.Machines = assetList.ToArray();
        filter.SetDesignMachineCellpassState(true);
      }

      filter.ReturnEarliestFilteredCellPass = (pdf != null) && pdf.ReturnEarliest.HasValue && pdf.ReturnEarliest.Value;

      if (pdf.AutomaticsType.HasValue)
      {
        filter.GCSGuidanceMode = (TGCSAutomaticsMode)pdf.AutomaticsType.Value;
        filter.SetGCSGuidanceModeCellpassState(true);
      }

      if (pdf.TemperatureRangeMin.HasValue && pdf.TemperatureRangeMax.HasValue)
      {
        filter.TemperatureRangeMin = (ushort)(pdf.TemperatureRangeMin.Value * 10);
        filter.TemperatureRangeMax = (ushort)(pdf.TemperatureRangeMax.Value * 10);
        filter.SetTemperatureRangeState(true);
      }

      if (pdf.PassCountRangeMin.HasValue && pdf.PassCountRangeMax.HasValue)
      {
        filter.PassCountRangeMin = pdf.PassCountRangeMin.Value;
        filter.PassCountRangeMax = pdf.PassCountRangeMax.Value;
        filter.SetPassCountRangeState(true);
      }

      //  log?.LogDebug($"Filter to be sent to Raptor: {JsonConvert.SerializeObject(filter)}");

      return filter;
    }

    public static TICLiftBuildSettings ConvertLift(LiftBuildSettings settings, TFilterLayerMethod layerMethod)
    {
      TICLiftBuildSettings result;
      result = settings == null ?
      new TSVOICOptions().GetLiftBuildSettings(layerMethod) :
      new TICLiftBuildSettings
      {
        CCVRange = ConvertCCVRange(settings.CCVRange),
        CCVSummarizeTopLayerOnly = settings.CCVSummarizeTopLayerOnly,
        DeadBandLowerBoundary = settings.DeadBandLowerBoundary,
        DeadBandUpperBoundary = settings.DeadBandUpperBoundary,
        FirstPassThickness = settings.FirstPassThickness,
        LiftDetectionType = ConvertLiftDetectionType(settings.LiftDetectionType),
        LiftThicknessType = ConvertLiftThicknessType(settings.LiftThicknessType),
        MDPRange = ConvertMDPRange(settings.MDPRange),
        MDPSummarizeTopLayerOnly = settings.MDPSummarizeTopLayerOnly,
        OverrideMachineCCV = settings.OverridingMachineCCV.HasValue,
        OverrideMachineMDP = settings.OverridingMachineMDP.HasValue,
        OverrideTargetPassCount = settings.OverridingTargetPassCountRange != null,
        OverrideTemperatureWarningLevels = settings.OverridingTemperatureWarningLevels != null,
        OverridingLiftThickness = settings.OverridingLiftThickness.HasValue ? settings.OverridingLiftThickness.Value : 0f,
        OverridingMachineCCV = settings.OverridingMachineCCV.HasValue ? settings.OverridingMachineCCV.Value : (short)0,
        OverridingMachineMDP = settings.OverridingMachineMDP.HasValue ? settings.OverridingMachineMDP.Value : (short)0,
        OverridingTargetPassCountRange = ConvertTargetPassCountRange(settings.OverridingTargetPassCountRange),
        OverridingTemperatureWarningLevels = ConvertTemperatureWarningLevels(settings.OverridingTemperatureWarningLevels),
        IncludeSuperseded = settings.IncludeSupersededLifts ?? false,
        TargetLiftThickness = settings.LiftThicknessTarget != null ? settings.LiftThicknessTarget.TargetLiftThickness : SVOICDecls.__Global.kICNullHeight,
        AboveToleranceLiftThickness = settings.LiftThicknessTarget != null ? settings.LiftThicknessTarget.AboveToleranceLiftThickness : 0,
        BelowToleranceLiftThickness = settings.LiftThicknessTarget != null ? settings.LiftThicknessTarget.BelowToleranceLiftThickness : 0,
        TargetMaxMachineSpeed = settings.MachineSpeedTarget != null ? (ushort)settings.MachineSpeedTarget.MaxTargetMachineSpeed : (ushort)0,
        TargetMinMachineSpeed = settings.MachineSpeedTarget != null ? (ushort)settings.MachineSpeedTarget.MinTargetMachineSpeed : (ushort)0,
      };
      if (settings != null)
        if (settings.CCvSummaryType != null)
          result.CCVSummaryTypes = result.CCVSummaryTypes.Set((int)settings.CCvSummaryType);
      return result;
    }


    public static TCCVRangePercentage ConvertCCVRange(CCVRangePercentage range)
    {
      return range == null ? new TCCVRangePercentage { Min = 0, Max = 100 } : new TCCVRangePercentage { Min = range.Min, Max = range.Max };
    }

    public static TMDPRangePercentage ConvertMDPRange(MDPRangePercentage range)
    {
      return range == null ? new TMDPRangePercentage { Min = 0, Max = 100 } : new TMDPRangePercentage { Min = range.Min, Max = range.Max };
    }

    public static TTargetPassCountRange ConvertTargetPassCountRange(TargetPassCountRange range)
    {
      return range == null ? new TTargetPassCountRange { Min = 1, Max = ushort.MaxValue } : new TTargetPassCountRange { Min = range.Min, Max = range.Max };
    }

    public static TICLiftDetectionType ConvertLiftDetectionType(LiftDetectionType type)
    {
      switch (type)
      {
        case LiftDetectionType.None: return TICLiftDetectionType.icldtNone;
        case LiftDetectionType.Automatic: return TICLiftDetectionType.icldtAutomatic;
        case LiftDetectionType.MapReset: return TICLiftDetectionType.icldtMapReset;
        case LiftDetectionType.AutoMapReset: return TICLiftDetectionType.icldtAutoMapReset;
        case LiftDetectionType.Tagfile: return TICLiftDetectionType.icldtTagfile;
        default: throw new Exception($"Unknown lift detection type: {Convert.ToInt32(type)}");
      }
    }

    public static int ConvertLiftThicknessType(LiftThicknessType type)
    {
      switch (type)
      {
        case LiftThicknessType.Compacted: return __Global.lttCompacted;
        case LiftThicknessType.Uncompacted: return __Global.lttUncompacted;
        default: throw new Exception($"Unknown lift thickness type: {Convert.ToInt32(type)}");
      }
    }

    public static TTemperatureWarningLevels ConvertTemperatureWarningLevels(TemperatureWarningLevels levels)
    {
      return levels == null ? new TTemperatureWarningLevels { Min = 0, Max = 100 } : new TTemperatureWarningLevels { Min = levels.Min, Max = levels.Max };
    }

    public static TICElevationType ConvertElevationType(ElevationType type)
    {
      switch (type)
      {
        case ElevationType.First: return TICElevationType.etFirst;
        case ElevationType.Last: return TICElevationType.etLast;
        case ElevationType.Highest: return TICElevationType.etHighest;
        case ElevationType.Lowest: return TICElevationType.etLowest;
        default: throw new Exception($"Unknown elevation type: {Convert.ToInt32(type)}");
      }
    }

    public static TFilterLayerMethod ConvertLayerMethod(FilterLayerMethod method)
    {
      switch (method)
      {
        case FilterLayerMethod.Invalid: return TFilterLayerMethod.flmInvalid;
        case FilterLayerMethod.None: return TFilterLayerMethod.flmNone;
        case FilterLayerMethod.AutoMapReset: return TFilterLayerMethod.flmAutoMapReset;
        case FilterLayerMethod.Automatic: return TFilterLayerMethod.flmAutomatic;
        case FilterLayerMethod.MapReset: return TFilterLayerMethod.flmMapReset;
        case FilterLayerMethod.OffsetFromBench: return TFilterLayerMethod.flmOffsetFromBench;
        case FilterLayerMethod.OffsetFromDesign: return TFilterLayerMethod.flmOffsetFromDesign;
        case FilterLayerMethod.OffsetFromProfile: return TFilterLayerMethod.flmOffsetFromProfile;
        case FilterLayerMethod.TagfileLayerNumber: return TFilterLayerMethod.flmTagfileLayerNumber;
        default: throw new Exception($"Unknown layer method: {Convert.ToInt32(method)}");
      }
    }

    public static TVLPDDesignDescriptor DesignDescriptor(long designID, string filespaceId, string path, string fileName, double offset)
    {
      return VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(designID, "RaptorServices", filespaceId, path, fileName, offset);
    }

    public static TVLPDDesignDescriptor EmptyDesignDescriptor => DesignDescriptor(0, string.Empty, string.Empty, string.Empty, 0);

    public static TVLPDDesignDescriptor DesignDescriptor(DesignDescriptor dd)
    {
      if (dd == null)
      {
        return EmptyDesignDescriptor;
      }

      return dd.File == null
        ? DesignDescriptor(dd.Id, string.Empty, string.Empty, string.Empty, dd.Offset)
        : DesignDescriptor(dd.Id, dd.File.filespaceId, dd.File.path, dd.File.fileName, dd.Offset);
    }

    /// <summary>
    /// ConvertVolumesType
    /// </summary>
    /// <param name="volumesType"></param>
    /// <returns></returns>
    public static TComputeICVolumesType ConvertVolumesType(VolumesType volumesType)
    {
      switch (volumesType)
      {
        case VolumesType.None: return TComputeICVolumesType.ic_cvtNone;
        case VolumesType.AboveLevel: return TComputeICVolumesType.ic_cvtAboveLevel;
        case VolumesType.Between2Levels: return TComputeICVolumesType.ic_cvtBetween2Levels;
        case VolumesType.AboveFilter: return TComputeICVolumesType.ic_cvtAboveFilter;
        case VolumesType.Between2Filters: return TComputeICVolumesType.ic_cvtBetween2Filters;
        case VolumesType.BetweenFilterAndDesign: return TComputeICVolumesType.ic_cvtBetweenFilterAndDesign;
        case VolumesType.BetweenDesignAndFilter: return TComputeICVolumesType.ic_cvtBetweenDesignAndFilter;
        default: throw new Exception($"Unknown VolumesType {Convert.ToInt16(volumesType)}");
      }
    }
    /// <summary>
    /// convertSurveyedSurfaceExlusionList
    /// </summary>
    /// <param name="exclusions"></param>
    /// <returns></returns>
    public static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(long[] exclusions)
    {
      if (exclusions == null) return new TSurveyedSurfaceID[0];
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Length];
      for (int i = 0; i < exclusions.Length; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }
    /// <summary>
    /// convertSurveyedSurfaceExlusionList
    /// </summary>
    /// <param name="exclusions"></param>
    /// <returns></returns>
    public static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(List<long> exclusions)
    {
      if (exclusions == null) return new TSurveyedSurfaceID[0];
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Count];
      for (int i = 0; i < exclusions.Count; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }

    /// <summary>
    /// convertCellAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static CellAddress convertCellAddress(TSubGridCellAddress address)
    {
      return CellAddress.CreateCellAddress(address.X, address.Y);
    }

    /// <summary>
    /// convertCellAddress
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static TSubGridCellAddress convertCellAddress(CellAddress address)
    {
      return new TSubGridCellAddress
      {
        X = address.x,
        Y = address.y
      };
    }

    public static TSVOICOptions convertOptions(ColorSettingsFlags colorSettingsFlags,
                                               LiftBuildSettings liftSettings,
                                               double volumesNoChangeTolerance,
                                               FilterLayerMethod filterLayerMethod,
                                               DisplayMode mode,
                                               bool setSummaryDataLayersVisibility)
    {
      TSVOICOptions options = new TSVOICOptions();

      if (colorSettingsFlags == null)
      {
        colorSettingsFlags = new ColorSettingsFlags();
        if (setSummaryDataLayersVisibility)
        {
          colorSettingsFlags.ccvSummaryWorkInProgressLayerVisible = true;
          colorSettingsFlags.ccvSummaryTooThickLayerVisible = true;
          colorSettingsFlags.mdpSummaryWorkInProgressLayerVisible = true;
          colorSettingsFlags.mdpSummaryTooThickLayerVisible = true;
        }
      };

      if (colorSettingsFlags != null && liftSettings != null)
      {
        // Important Note! due to some values been used in a 10th precision in the backend raptor
        // e.g. a user cmv override target setting value of 100 is actually has a value of 1000 in raptor
        // therefore some settings get multiplied by 10. Ideally it would have been stored in that precision but it's too late now

        options.AbsoluteTargetCCV = liftSettings.OverridingMachineCCV.HasValue ? liftSettings.OverridingMachineCCV.Value : (short)0;
        ////(short)Math.Round(liftSettings.overridingMachineCCV.Value * 10);

        options.CCVDecouplingColour = (int)Colors.Black;
        options.CCVRange = ConvertCCVRange(liftSettings.CCVRange);
        options.CCVSummarizeTopLayerOnly = liftSettings.CCVSummarizeTopLayerOnly;
        options.CCVTargetColour = (int)Colors.Green;
        options.FirstPassThickness = (float)liftSettings.FirstPassThickness;
        switch (filterLayerMethod)
        {
          case FilterLayerMethod.None:
            options.LiftDetectionType = TICLiftDetectionType.icldtNone;
            break;
          case FilterLayerMethod.Automatic:
            options.LiftDetectionType = TICLiftDetectionType.icldtAutomatic;
            break;
          case FilterLayerMethod.MapReset:
            options.LiftDetectionType = TICLiftDetectionType.icldtMapReset;
            break;
          case FilterLayerMethod.TagfileLayerNumber:
            options.LiftDetectionType = TICLiftDetectionType.icldtTagfile;
            break;
          case FilterLayerMethod.AutoMapReset:
          default:
            options.LiftDetectionType = TICLiftDetectionType.icldtAutoMapReset;
            break;
        }
        options.LiftThicknessMachine = !liftSettings.OverridingLiftThickness.HasValue;
        options.LiftThicknessType = ConvertLiftThicknessType(liftSettings.LiftThicknessType);
        options.MaximumLiftThickness = liftSettings.OverridingLiftThickness.HasValue ? liftSettings.OverridingLiftThickness.Value : 0f;

        options.PassTargetColour = (int)Colors.Green;
        options.TargetPassCountRange = ConvertTargetPassCountRange(liftSettings.OverridingTargetPassCountRange);
        options.UseMachineTargetCCV = !liftSettings.OverridingMachineCCV.HasValue;
        options.UseMachineTargetPass = liftSettings.OverridingTargetPassCountRange == null;

        options.SetCCVSummaryTypeWIP(colorSettingsFlags.ccvSummaryWorkInProgressLayerVisible);
        options.SetCCVSummaryTypeThickness(colorSettingsFlags.ccvSummaryTooThickLayerVisible);

        if (mode == DisplayMode.TargetThicknessSummary)
        {
          options.TargetLiftThickness = liftSettings.LiftThicknessTarget.TargetLiftThickness;
          options.AboveToleranceLiftThickness = liftSettings.LiftThicknessTarget.AboveToleranceLiftThickness;
          options.BelowToleranceLiftThickness = liftSettings.LiftThicknessTarget.BelowToleranceLiftThickness;
        }
        else
          options.TargetLiftThickness = SVOICDecls.__Global.kICNullHeight;

        options.MinSpeedTarget = mode == DisplayMode.TargetSpeedSummary ? liftSettings.MachineSpeedTarget.MinTargetMachineSpeed : SVOICDecls.__Global.kICNullMachineSpeed;
        options.MaxSpeedTarget = mode == DisplayMode.TargetSpeedSummary ? liftSettings.MachineSpeedTarget.MaxTargetMachineSpeed : SVOICDecls.__Global.kICNullMachineSpeed;

        switch (mode) // for summary modes make sure at least compaction is set
        {
          case DisplayMode.CCVSummary:
          case DisplayMode.CCVPercentSummary:
            options.SetCCVSummaryTypeCompaction(true);  // always set
            break;

          case DisplayMode.MDPSummary:
          case DisplayMode.MDPPercentSummary:
            options.SetMDPSummaryTypeCompaction(true); // always set
            break;
        };

        options.NoChangeVolumeTolerance = (float)volumesNoChangeTolerance;

        options.UseMachineTempWarningLevels = liftSettings.OverridingTemperatureWarningLevels == null;
        if (liftSettings.OverridingTemperatureWarningLevels != null)
        {
          options.TemperatureWarningLevels =
              ConvertTemperatureWarningLevels(liftSettings.OverridingTemperatureWarningLevels);
        }

        options.AbsoluteTargetMDP = liftSettings.OverridingMachineMDP.HasValue ? liftSettings.OverridingMachineMDP.Value : (short)0;
        //(short)Math.Round(liftSettings.overrideTargetMDPValue.Value * 10);
        options.MDPRange = ConvertMDPRange(liftSettings.MDPRange);
        options.MDPSummarizeTopLayerOnly = liftSettings.MDPSummarizeTopLayerOnly;
        options.MDPTargetColour = (int)Colors.Green;
        options.UseMachineTargetMDP = !liftSettings.OverridingMachineMDP.HasValue;
        options.SetMDPSummaryTypeWIP(colorSettingsFlags.mdpSummaryWorkInProgressLayerVisible);
        options.SetMDPSummaryTypeThickness(colorSettingsFlags.mdpSummaryTooThickLayerVisible);

        options.DeadBandLowerBoundary = liftSettings.DeadBandLowerBoundary;
        options.DeadBandUpperBoundary = liftSettings.DeadBandUpperBoundary;

        options.IncludeSupersededLayers = liftSettings.IncludeSupersededLifts ?? false;
      }

      return options;
    }

    public static void convertGridOrLLBoundingBox(BoundingBox2DGrid grid, BoundingBox2DLatLon ll, out TWGS84Point bl,
        out TWGS84Point tr, out bool coordsAreGrid)
    {
      coordsAreGrid = grid != null;
      if (coordsAreGrid)
      {
        bl = TWGS84Point.PointXY(grid.BottomLeftX, grid.BottomleftY);
        tr = TWGS84Point.PointXY(grid.TopRightX, grid.TopRightY);
      }
      else if (ll != null)
      {
        bl = TWGS84Point.Point(ll.BottomLeftLon, ll.BottomLeftLat);
        tr = TWGS84Point.Point(ll.TopRightLon, ll.TopRightLat);
      }
      else
      {
        bl = TWGS84Point.Point(0, 0);
        tr = TWGS84Point.Point(0, 0);
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings topFilter,
                                                                  DisplayMode mode,
                                                                  VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if (((mode == DisplayMode.CutFill) || (mode == DisplayMode.VolumeCoverage) || (mode == DisplayMode.TargetThicknessSummary))
          &&
          ((computeVolType == VolumesType.BetweenDesignAndFilter) || (computeVolType == VolumesType.BetweenFilterAndDesign)))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings topFilter,
                                                                  VolumesType computeVolType)
    {
      // Adjust filter to take into account volume computations with respect to designs
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if ((computeVolType == VolumesType.BetweenDesignAndFilter) || (computeVolType == VolumesType.BetweenFilterAndDesign))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured filter2 for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings filter1,
                                                                  ref TICFilterSettings filter2,
                                                                  DisplayMode mode,
                                                                  VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests invovle a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests
      if (((mode == DisplayMode.CutFill) || (mode == DisplayMode.VolumeCoverage) || (mode == DisplayMode.TargetThicknessSummary)))
      {
        if (computeVolType == VolumesType.BetweenDesignAndFilter)
        {
          // Force topfilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          filter2 = new TICFilterSettings();
        }

        if (computeVolType == VolumesType.BetweenFilterAndDesign)
        {
          // Force basefilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          filter2 = new TICFilterSettings();
        }
      }
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter or baseFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static void reconcileTopFilterAndVolumeComputationMode(ref TICFilterSettings baseFilter,
                                                                  ref TICFilterSettings topFilter,
                                                                  VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations respect to designs. 
      // If these requests invovle a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests

      if (computeVolType == VolumesType.BetweenDesignAndFilter)
      {
        // Force topfilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        baseFilter = new TICFilterSettings();
      }

      if (computeVolType == VolumesType.BetweenFilterAndDesign)
      {
        // Force basefilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        topFilter = new TICFilterSettings();
      }
    }

  }
}
