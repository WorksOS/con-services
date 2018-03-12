using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  ///   Default settings for compaction end points. For consistency all compaction end points should use these settings.
  ///   They should be passed to Raptor for tiles and for retrieving data and also returned to the client UI (albeit in a
  ///   simplfied form).
  /// </summary>
  public class CompactionSettingsManager : ICompactionSettingsManager
  {
    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;

    private const short MIN_CMV_MDP_VALUE = 0;
    private const short MAX_CMV_MDP_VALUE = 2000;

    public LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
    }

    public CMVSettings CompactionCmvSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
    }

    public MDPSettings CompactionMdpSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
    }

    public TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings ps, bool nativeValues = true)
    {
      return AutoMapperUtility.Automapper.Map<TemperatureSettings>(ps);
    }

    public double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CmvPercentChangeSettings>(ps).percents;
    }

    public PassCountSettings CompactionPassCountSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<PassCountSettings>(ps);
    }

    public double[] CompactionCutFillSettings(CompactionProjectSettings ps)
    {
      return AutoMapperUtility.Automapper.Map<CutFillSettings>(ps).percents;
    }

    public List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors)
    {
      var palette = new List<ColorPalette>();

      bool useDefaultValue;
      uint underColor;
      uint onColor;
      uint overColor;

      switch (mode)
      {
        case DisplayMode.Height:

          if (elevExtents == null)
          {
            palette = null;
          }
          else
          {
            //Compaction elevation palette has 31 colors, original Raptor one had 30 colors
            var colors = projectSettingsColors.useDefaultElevationColors.HasValue &&
                         projectSettingsColors.useDefaultElevationColors.Value
              ? CompactionProjectSettingsColors.DefaultSettings.elevationColors
              : projectSettingsColors.elevationColors;
            var step = (elevExtents.MaxElevation - elevExtents.MinElevation) / (colors.Count - 1);

            palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationBelowColor, -1));
            for (var i = 0; i < colors.Count; i++)
            {
              palette.Add(ColorPalette.CreateColorPalette((uint) colors[i], elevExtents.MinElevation + i * step));
            }
            palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.elevationAboveColor, -1));
          }

          break;
        case DisplayMode.CCV:
          const int STEP = 100;
          var cmvColors = projectSettingsColors.useDefaultCMVDetailsColors.HasValue &&
                          projectSettingsColors.useDefaultCMVDetailsColors.Value
            ? CompactionProjectSettingsColors.DefaultSettings.cmvDetailsColors
            : projectSettingsColors.cmvDetailsColors;

          for (var i = 0; i < cmvColors.Count; i++)
          {
            //The last 16th color and value are for above...
            palette.Add(ColorPalette.CreateColorPalette(cmvColors[i], i * STEP));
          }
          break;
        case DisplayMode.PassCount:
          var passCountSettings = CompactionPassCountSettings(projectSettings);
          var passCountDetailColors = projectSettingsColors.useDefaultPassCountDetailsColors.HasValue &&
                                      projectSettingsColors.useDefaultPassCountDetailsColors.Value
            ? CompactionProjectSettingsColors.DefaultSettings.passCountDetailsColors
            : projectSettingsColors.passCountDetailsColors;

          for (var i = 0; i < passCountSettings.passCounts.Length; i++)
          {
            //The colors and values for 1-8
            palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[i], passCountSettings.passCounts[i]));
          }
          //The 9th color and value (for above)
          palette.Add(ColorPalette.CreateColorPalette(passCountDetailColors[8], passCountSettings.passCounts[7] + 1));
          break;
        case DisplayMode.PassCountSummary:
          //Values don't matter here as no machine override for compaction
          useDefaultValue = projectSettingsColors.useDefaultPassCountSummaryColors.HasValue &&
                            projectSettingsColors.useDefaultPassCountSummaryColors.Value;

          underColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.passCountUnderTargetColor.Value
            : projectSettingsColors.passCountUnderTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.passCountUnderTargetColor.Value;

          onColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.passCountOnTargetColor.Value
            : projectSettingsColors.passCountOnTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.passCountOnTargetColor.Value;

          overColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.passCountOverTargetColor.Value
            : projectSettingsColors.passCountOverTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.passCountOverTargetColor.Value;

          palette.Add(ColorPalette.CreateColorPalette(underColor, ColorSettings.Default.passCountMinimum.value));
          palette.Add(ColorPalette.CreateColorPalette(onColor, ColorSettings.Default.passCountTarget.value));
          palette.Add(ColorPalette.CreateColorPalette(overColor, ColorSettings.Default.passCountMaximum.value));
          break;
        case DisplayMode.CutFill:
          //Note: cut-fill also requires a design for tile requests 
          var cutFillTolerances = CompactionCutFillSettings(projectSettings);
          var cutFillColors = projectSettingsColors.useDefaultCutFillColors.HasValue &&
                              projectSettingsColors.useDefaultCutFillColors.Value
            ? CompactionProjectSettingsColors.DefaultSettings.cutFillColors
            : projectSettingsColors.cutFillColors;

          for (var i = 0; i < cutFillColors.Count; i++)
          {
            palette.Add(ColorPalette.CreateColorPalette(cutFillColors[i], cutFillTolerances[i]));
          }
          break;
        case DisplayMode.TemperatureSummary:
          useDefaultValue = projectSettingsColors.useDefaultTemperatureSummaryColors.HasValue &&
                            projectSettingsColors.useDefaultTemperatureSummaryColors.Value;

          underColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.temperatureUnderTargetColor.Value
            : projectSettingsColors.temperatureUnderTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.temperatureUnderTargetColor.Value;

          onColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.temperatureOnTargetColor.Value
            : projectSettingsColors.temperatureOnTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.temperatureOnTargetColor.Value;

          overColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.temperatureOverTargetColor.Value
            : projectSettingsColors.temperatureOverTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.temperatureOverTargetColor.Value;

          palette.Add(ColorPalette.CreateColorPalette(underColor, 0));
          palette.Add(ColorPalette.CreateColorPalette(onColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(overColor, 2));
          break;
        case DisplayMode.CCVPercentSummary:
          useDefaultValue = projectSettingsColors.useDefaultCMVSummaryColors.HasValue &&
                            projectSettingsColors.useDefaultCMVSummaryColors.Value;

          underColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.cmvUnderTargetColor.Value
            : projectSettingsColors.cmvUnderTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.cmvUnderTargetColor.Value;

          onColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.cmvOnTargetColor.Value
            : projectSettingsColors.cmvOnTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.cmvOnTargetColor.Value;

          overColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.cmvOverTargetColor.Value
            : projectSettingsColors.cmvOverTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.cmvOverTargetColor.Value;

          palette.Add(ColorPalette.CreateColorPalette(onColor, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(underColor, 2));
          palette.Add(ColorPalette.CreateColorPalette(overColor, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.ccvSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.MDPPercentSummary:
          useDefaultValue = projectSettingsColors.useDefaultMDPSummaryColors.HasValue &&
                            projectSettingsColors.useDefaultMDPSummaryColors.Value;

          underColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.mdpUnderTargetColor.Value
            : projectSettingsColors.mdpUnderTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.mdpUnderTargetColor.Value;

          onColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.mdpOnTargetColor.Value
            : projectSettingsColors.mdpOnTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.mdpOnTargetColor.Value;

          overColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.mdpOverTargetColor.Value
            : projectSettingsColors.mdpOverTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.mdpOverTargetColor.Value;

          palette.Add(ColorPalette.CreateColorPalette(onColor, 0));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryWorkInProgressLayerColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(underColor, 2));
          palette.Add(ColorPalette.CreateColorPalette(overColor, 3));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryTooThickLayerColor, 4));
          palette.Add(ColorPalette.CreateColorPalette(ColorSettings.Default.mdpSummaryApprovedLayerColor, 5));
          break;
        case DisplayMode.TargetSpeedSummary:
          useDefaultValue = projectSettingsColors.useDefaultSpeedSummaryColors.HasValue &&
                            projectSettingsColors.useDefaultSpeedSummaryColors.Value;

          underColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.speedUnderTargetColor.Value
            : projectSettingsColors.speedUnderTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.speedUnderTargetColor.Value;

          onColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.speedOnTargetColor.Value
            : projectSettingsColors.speedOnTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.speedOnTargetColor.Value;

          overColor = useDefaultValue
            ? CompactionProjectSettingsColors.DefaultSettings.speedOverTargetColor.Value
            : projectSettingsColors.speedOverTargetColor ??
              CompactionProjectSettingsColors.DefaultSettings.speedOverTargetColor.Value;

          palette.Add(ColorPalette.CreateColorPalette(underColor, 0));
          palette.Add(ColorPalette.CreateColorPalette(onColor, 1));
          palette.Add(ColorPalette.CreateColorPalette(overColor, 2));
          break;
        case DisplayMode.CMVChange:
          var cmvPercentChangeSettings = CompactionCmvPercentChangeSettings(projectSettings);
          var cmvPercentChangeColors = projectSettingsColors.useDefaultCMVPercentColors.HasValue &&
                                       projectSettingsColors.useDefaultCMVPercentColors.Value
            ? CompactionProjectSettingsColors.DefaultSettings.cmvPercentColors
            : projectSettingsColors.cmvPercentColors;

          palette.Add(ColorPalette.CreateColorPalette(Colors.None, double.MinValue));

          for (var i = 0; i < cmvPercentChangeSettings.Length; i++)
            palette.Add(ColorPalette.CreateColorPalette(cmvPercentChangeColors[i], cmvPercentChangeSettings[i]));

          palette.Add(ColorPalette.CreateColorPalette(cmvPercentChangeColors[cmvPercentChangeColors.Count - 1],
            NO_CCV));

          break;
      }
      return palette;
    }
  }
}