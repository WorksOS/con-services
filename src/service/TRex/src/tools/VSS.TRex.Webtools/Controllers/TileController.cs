using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/tiles")]
  public class TileController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Generates a tile.
    /// </summary>
    [HttpPost("{siteModelID}")]
    public async Task<JsonResult> GetTile(
      [FromRoute]string siteModelID,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY,
      [FromQuery] int mode,
      [FromQuery] ushort pixelsX,
      [FromQuery] ushort pixelsY,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] double? offset,
      [FromBody] OverrideParameters overrides)
    {
      var request = new TileRenderRequest();

      var siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var displayMode = (DisplayMode) mode;

      var response = await request.ExecuteAsync(new TileRenderRequestArgument(
        siteModelUid,
        displayMode,
        ConvertColorPalettes(displayMode, siteModel, overrides),
        new BoundingWorldExtent3D(minX, minY, maxX, maxY),
        true,
        pixelsX,
        pixelsY,
        new FilterSet(new CombinedFilter(), new CombinedFilter()),
        new DesignOffset(cutFillDesignUid ?? Guid.Empty, offset ?? 0.0)
      )) as TileRenderResponse_Core2;

      return new JsonResult(new TileResult(response?.TileBitmapData));
    }

    private PaletteBase ConvertColorPalettes(DisplayMode mode, ISiteModel siteModel, OverrideParameters overrides)
    {
      const double PERCENTAGE_RANGE_MIN = 0.0;
      const double PERCENTAGE_RANGE_MAX = 100.0;
      const ushort PASS_COUNT_TARGET_RANGE_MIN = 1;
      const ushort PASS_COUNT_TARGET_RANGE_MAX = ushort.MaxValue;
      const ushort TEMPERATURE_LEVELS_MIN = 0;
      const ushort TEMPERATURE_LEVELS_MAX = 100;

      PaletteBase convertedPalette;

      switch (mode)
      {
        case DisplayMode.CCA:
          convertedPalette = new CCAPalette();
          break;
        case DisplayMode.CCASummary:
          convertedPalette = new CCASummaryPalette();  
          break;
        case DisplayMode.CCV:
          convertedPalette = new CMVPalette();

          var cmvPalette = ((CMVPalette)convertedPalette);

          cmvPalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvPalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;

          cmvPalette.TargetCCVColour = Color.Green;
          cmvPalette.DefaultDecoupledCMVColour = Color.Black;
          break;
        case DisplayMode.CCVPercentSummary:
          convertedPalette = new CMVSummaryPalette();

          var cmvSummaryPalette = ((CMVSummaryPalette)convertedPalette);

          cmvSummaryPalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvSummaryPalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvSummaryPalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvSummaryPalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;
          break;
        case DisplayMode.CMVChange:
          convertedPalette = new CMVPercentChangePalette();

          var cmvPercentChangePalette = ((CMVPercentChangePalette)convertedPalette);

          cmvPercentChangePalette.CMVPercentageRange.Min = overrides?.CMVRange.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPercentChangePalette.CMVPercentageRange.Max = overrides?.CMVRange.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPercentChangePalette.UseAbsoluteValues = false;

          cmvPercentChangePalette.UseMachineTargetCMV = !overrides?.OverrideMachineCCV ?? true;
          cmvPercentChangePalette.AbsoluteTargetCMV = overrides?.OverridingMachineCCV ?? 0;

          cmvPercentChangePalette.TargetCCVColour = Color.Green;
          cmvPercentChangePalette.DefaultDecoupledCMVColour = Color.Black;
          break;
        case DisplayMode.CutFill:
          convertedPalette = new CutFillPalette();
          break;
        case DisplayMode.Height:
          var extent = siteModel.GetAdjustedDataModelSpatialExtents(new Guid[0]);

          convertedPalette = new HeightPalette(extent.MinZ, extent.MaxZ);
          break;
        case DisplayMode.MDP:
          convertedPalette = new MDPPalette();

          var mdpPalette = ((MDPPalette)convertedPalette);

          mdpPalette.MDPPercentageRange.Min = overrides?.MDPRange.Min ?? PERCENTAGE_RANGE_MIN;
          mdpPalette.MDPPercentageRange.Max = overrides?.MDPRange.Max ?? PERCENTAGE_RANGE_MAX;

          mdpPalette.UseMachineTargetMDP = !overrides?.OverrideMachineMDP ?? true;
          mdpPalette.AbsoluteTargetMDP = overrides?.OverridingMachineMDP ?? 0;

          mdpPalette.TargetMDPColour = Color.Green;
          break;
        case DisplayMode.MDPPercentSummary:
          convertedPalette = new MDPSummaryPalette();

          var mdpSummaryPalette = ((MDPSummaryPalette)convertedPalette);

          mdpSummaryPalette.MDPPercentageRange.Min = overrides?.MDPRange.Min ?? PERCENTAGE_RANGE_MIN;
          mdpSummaryPalette.MDPPercentageRange.Max = overrides?.MDPRange.Max ?? PERCENTAGE_RANGE_MAX;

          mdpSummaryPalette.UseMachineTargetMDP = !overrides?.OverrideMachineMDP ?? true;
          mdpSummaryPalette.AbsoluteTargetMDP = overrides?.OverridingMachineMDP ?? 0;
          break;
        case DisplayMode.PassCount:
          convertedPalette = new PassCountPalette();
          break;
        case DisplayMode.PassCountSummary:
          convertedPalette = new PassCountSummaryPalette();

          var passCountPalette = ((PassCountSummaryPalette)convertedPalette);

          passCountPalette.UseMachineTargetPass = !overrides?.OverrideTargetPassCount ?? true;
          passCountPalette.TargetPassCountRange.Min = overrides?.OverridingTargetPassCountRange.Min ?? PASS_COUNT_TARGET_RANGE_MIN;
          passCountPalette.TargetPassCountRange.Max = overrides?.OverridingTargetPassCountRange.Max ?? PASS_COUNT_TARGET_RANGE_MAX;
          break;
        case DisplayMode.MachineSpeed:
          convertedPalette = new SpeedPalette();
          break;
        case DisplayMode.TargetSpeedSummary:
          convertedPalette = new SpeedSummaryPalette();

          var speedSummaryPalette = ((SpeedSummaryPalette)convertedPalette);

          speedSummaryPalette.MachineSpeedTarget.Min = overrides?.TargetMachineSpeed.Min ?? CellPassConsts.NullMachineSpeed;
          speedSummaryPalette.MachineSpeedTarget.Max = overrides?.TargetMachineSpeed.Max ?? CellPassConsts.NullMachineSpeed;
          break;
        case DisplayMode.TemperatureDetail:
          convertedPalette = new TemperaturePalette();
          break;
        case DisplayMode.TemperatureSummary:
          convertedPalette = new TemperatureSummaryPalette();

          var temperatureSummaryPalette = ((TemperatureSummaryPalette)convertedPalette);

          temperatureSummaryPalette.UseMachineTempWarningLevels = !overrides?.OverrideTemperatureWarningLevels ?? true;
          temperatureSummaryPalette.TemperatureLevels.Min = overrides?.OverridingTemperatureWarningLevels.Min ?? TEMPERATURE_LEVELS_MIN;
          temperatureSummaryPalette.TemperatureLevels.Max = overrides?.OverridingTemperatureWarningLevels.Max ?? TEMPERATURE_LEVELS_MAX;
          break;
        default:
          throw new TRexException($"No implemented colour palette for this mode ({mode})");
      }

      return convertedPalette;
    }

    /// <summary>
    /// Retrieves the list of available tile generation modes
    /// </summary>
    /// <returns></returns>
    [HttpGet("modes")]
    public JsonResult GetModes()
    {
      return new JsonResult(new List<(DisplayMode Index, string Name)>
      {
        (DisplayMode.Height, "Height"),
        (DisplayMode.CCV, "CCV"),
        (DisplayMode.CCVPercentSummary, "CCV Summary"),
        (DisplayMode.CMVChange, "CCV Change"),
        (DisplayMode.PassCount, "Pass Count"),
        (DisplayMode.PassCountSummary, "Pass Count Summary"),
        (DisplayMode.MDPPercentSummary, "MDP Summary"),
        (DisplayMode.CutFill, "Cut/Fill"),
        (DisplayMode.MachineSpeed, "Speed"),
        (DisplayMode.TargetSpeedSummary, "Speed Summary"),
        (DisplayMode.TemperatureSummary, "Temperature Summary"),
        (DisplayMode.CCA, "CCA"),
        (DisplayMode.CCASummary, "CCA Summary")
      });
    }
  }
}
