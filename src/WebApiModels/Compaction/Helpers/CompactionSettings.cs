using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Default settings for compaction end points. For consistency all compaction end points should use these settings.
  /// They should be passed to Raptor for tiles and for retrieving data and also returned to the client UI (albeit in a simplfied form).
  /// </summary>
  public static class CompactionSettings
  {
    public static LiftBuildSettings CompactionLiftBuildSettings
    {
      get
      {
        try
        {
          return JsonConvert.DeserializeObject<LiftBuildSettings>(
            "{'liftDetectionType': '4', 'machineSpeedTarget': { 'MinTargetMachineSpeed': '333', 'MaxTargetMachineSpeed': '417'}}");
          //liftDetectionType 4 = None, speeds are cm/sec (12 - 15 km/hr)        
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              ex.Message));
        }
      }
    }

    public static Filter CompactionFilter(DateTime? startUtc, DateTime? endUtc, long? onMachineDesignId, bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, List<MachineDetails> machines)
    {
      bool haveFilter = startUtc.HasValue || endUtc.HasValue || onMachineDesignId.HasValue ||
                  vibeStateOn.HasValue || elevationType.HasValue || layerNumber.HasValue || (machines != null && machines.Count > 0);

      var layerMethod = layerNumber.HasValue ? FilterLayerMethod.TagfileLayerNumber : FilterLayerMethod.None;

      return haveFilter ? 
        Filter.CreateFilter(null, null, null, startUtc, endUtc, onMachineDesignId, null, vibeStateOn, null, elevationType,
         null, null, null, null, null, null, null, null, null, layerMethod, null, null, layerNumber, null, machines, 
         null, null, null, null, null, null, null) 
         : null;
    }

    public static CMVSettings CompactionCmvSettings
    {
      get
      {
        //Note: CMVSettings documentation raw values are 10ths
        return CMVSettings.CreateCMVSettings(700, 1000, 120, 200, 80, false);
      }
    }

    public static MDPSettings CompactionMdpSettings
    {
      get
      {
        //Note: MDPSettings documentation raw values are 10ths
        return MDPSettings.CreateMDPSettings(700, 1000, 120, 200, 80, false);
      }
    }

    public static TemperatureSettings CompactionTemperatureSettings
    {
      get
      {
        //Temperature settings are degrees Celcius (but temperature warning levels for override are 10ths)
        return TemperatureSettings.CreateTemperatureSettings(175, 65, false);
      }
    }

    public static double[] CompactionCmvPercentChangeSettings
    {
      get
      {
        //return new double[] { 5, 20, 50, NO_CCV };
        return new double[] { 5, 20, 50 };
      }
    }

    public static PassCountSettings CompactionPassCountSettings
    {
      get
      {
        return PassCountSettings.CreatePassCountSettings(new int[] {1,2,3,4,5,6,7,8});
      }
    }

    public static List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents)
    {
      const uint OVER_COLOR = 0xD50000;
      const uint ON_COLOR = 0x8BC34A;
      const uint UNDER_COLOR = 0x1579B;

      List<ColorPalette> palette = new List<ColorPalette>();
      switch (mode)
      {
        case DisplayMode.Height:

          const int NUMBER_OF_COLORS = 30;

          double step = (elevExtents.MaxElevation - elevExtents.MinElevation) / (NUMBER_OF_COLORS - 1);
          List<int> colors = RaptorConverters.ElevationPalette();

          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationBelowColor, -1));
          for (int i = 0; i < colors.Count; i++)
          {
            palette.Add(ColorPalette.CreateColorPalette((uint)colors[i], elevExtents.MinElevation + i * step));
          }
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationAboveColor, -1));

          break;
        case DisplayMode.CCV:
          CMVSettings cmvSettings = CompactionCmvSettings;
          palette.Add(ColorPalette.CreateColorPalette(Colors.Gray, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.cmvMinimum.color, cmvSettings.minCMV/10.0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.cmvTarget.color, cmvSettings.cmvTarget/10.0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.cmvTarget.color, cmvSettings.cmvTarget/10.0 + 1));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.cmvMaximum.color, cmvSettings.maxCMV/10.0 + 1));
          break;
        case DisplayMode.PassCount:
          PassCountSettings passCountSettings = CompactionPassCountSettings;
          List<uint> passCountDetailColors = new List<uint>  { 0x2D5783, 0x439BDC, 0xBEDFF1, 0x9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037 };         
          for (int i = 0; i < passCountSettings.passCounts.Length; i++)
          {
            //The colors and values for 1-8
            palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[i], passCountSettings.passCounts[i]));
          }
          //The 9th color and value (for above)
          palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[8], passCountSettings.passCounts[7]+1));         
          break;
        case DisplayMode.PassCountSummary:
          //Values don't matter here as no machine override for compaction
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, ColorSettings.Default.passCountMinimum.value));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, ColorSettings.Default.passCountTarget.value));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, ColorSettings.Default.passCountMaximum.value));
          break;
        case DisplayMode.CutFill:
          //TODO: when cut-fill implemented, need to have CompactionSettings with cut/fill tolerance and use it here for values
          //Note: cut-fill also requires a design for tile requests (make cut-fill compaction settings ?)
          var cutFillDefaults = ColorSettings.Default.cutFillColors;
          List<uint> cutFillColors = new List<uint> { 0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0x01579B, 0x039BE5, 0xB3E5FC };
          for (int i = 0; i < cutFillColors.Count; i++)
          {
            palette.Add(ColorPalette.CreateColorPalette(cutFillColors[i], cutFillDefaults[i].value));
          }
          break;
        case DisplayMode.TemperatureSummary:
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 1));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 2));
          break;
        case DisplayMode.CCVPercentSummary:
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 2));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.MDPPercentSummary:
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 2));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.TargetSpeedSummary:
          palette.Add(ColorPalette.CreateColorPalette(UNDER_COLOR, 0));
          palette.Add(ColorPalette.CreateColorPalette(ON_COLOR, 1));
          palette.Add(ColorPalette.CreateColorPalette(OVER_COLOR, 2));
          break;
        case DisplayMode.CMVChange:
          var cmvPercentChangeSettings = CompactionCmvPercentChangeSettings;
          palette.Add(ColorPalette.CreateColorPalette(Colors.None, 0));
          palette.Add(ColorPalette.CreateColorPalette(0x8BC34A, cmvPercentChangeSettings[0]));
          palette.Add(ColorPalette.CreateColorPalette(0xFFCDD2, cmvPercentChangeSettings[1]));
          palette.Add(ColorPalette.CreateColorPalette(0xE57373, cmvPercentChangeSettings[2]));
          palette.Add(ColorPalette.CreateColorPalette(0xD50000, NO_CCV));
          break;
      }
      return palette;
    }

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;


  }
}
