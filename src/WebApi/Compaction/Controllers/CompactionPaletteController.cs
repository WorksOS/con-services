using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes;
using ColorValue = VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes.ColorValue;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting color palettes for displaying Raptor production data
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]

  public class CompactionPaletteController : BaseController
  {
    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionPaletteController(ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IElevationExtentsProxy elevProxy, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy,
      ICompactionSettingsManager settingsManager, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) :
      base(loggerFactory, loggerFactory.CreateLogger<CompactionPaletteController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.elevProxy = elevProxy;
    }

    /// <summary>
    /// Get color palettes for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <returns>Color palettes for all display types</returns>
    [ProjectUidVerifier]
    [Route("api/v2/colorpalettes")]
    [HttpGet]
    public async Task<CompactionColorPalettesResult> GetColorPalettes(
      [FromQuery] Guid projectUid)
    {
      Log.LogInformation("GetColorPalettes: " + Request.QueryString);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = await GetProjectSettingsColors(projectUid);

      //Note: elevation palette is a separate call as it requires a filter
      List<DisplayMode> modes = new List<DisplayMode>
      {
        DisplayMode.CCV,
        DisplayMode.PassCount,
        DisplayMode.PassCountSummary,
        DisplayMode.CutFill,
        DisplayMode.TemperatureSummary,
        DisplayMode.CCVPercentSummary,
        DisplayMode.MDPPercentSummary,
        DisplayMode.TargetSpeedSummary,
        DisplayMode.CMVChange
      };

      DetailPalette cmvDetailPalette = null;
      DetailPalette passCountDetailPalette = null;
      SummaryPalette passCountSummaryPalette = null;
      DetailPalette cutFillPalette = null;
      SummaryPalette temperatureSummaryPalette = null;
      SummaryPalette cmvSummaryPalette = null;
      SummaryPalette mdpSummaryPalette = null;
      DetailPalette cmvPercentChangePalette = null;
      SummaryPalette speedSummaryPalette = null;

      //This is temporary until temperature details implemented in Raptor.
      DetailPalette temperatureDetailPalette = DetailPalette.CreateDetailPalette(
        new List<ColorValue>
        {
          ColorValue.CreateColorValue(0x2D5783, 70),
          ColorValue.CreateColorValue(0x439BDC, 80),
          ColorValue.CreateColorValue(0xBEDFF1, 90),
          ColorValue.CreateColorValue(0xDCEEC7, 100),
          ColorValue.CreateColorValue(0x9DCE67, 110),
          ColorValue.CreateColorValue(0x6BA03E, 120),
          ColorValue.CreateColorValue(0x3A6B25, 130),
          ColorValue.CreateColorValue(0xF6CED3, 140),
          ColorValue.CreateColorValue(0xD57A7C, 150),
          ColorValue.CreateColorValue(0xC13037, 160)
        },
        null, null);

      foreach (var mode in modes)
      {
        List<ColorValue> colorValues;
        var compactionPalette = this.SettingsManager.CompactionPalette(mode, null, projectSettings, projectSettingsColors);
        switch (mode)
        {
          case DisplayMode.CCV:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                compactionPalette[i].value / 10));//Raptor CMV is 10ths but return actual CMV to UI
            }
            cmvDetailPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
          case DisplayMode.PassCount:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                compactionPalette[i].value));
            }
            passCountDetailPalette = DetailPalette.CreateDetailPalette(colorValues,
              compactionPalette[compactionPalette.Count - 1].color, null);
            break;
          case DisplayMode.PassCountSummary:
            passCountSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
              compactionPalette[1].color, compactionPalette[0].color);
            break;
          case DisplayMode.CutFill:
            colorValues = new List<ColorValue>();
            for (int i = compactionPalette.Count - 1; i >= 0; i--)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                compactionPalette[i].value));
            }
            cutFillPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
            break;
          case DisplayMode.TemperatureSummary:
            temperatureSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
              compactionPalette[1].color, compactionPalette[0].color);
            break;
          case DisplayMode.CCVPercentSummary:
            cmvSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].color,
              compactionPalette[0].color, compactionPalette[2].color);
            break;
          case DisplayMode.MDPPercentSummary:
            mdpSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].color,
              compactionPalette[0].color, compactionPalette[2].color);
            break;
          case DisplayMode.TargetSpeedSummary:
            speedSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
              compactionPalette[1].color, compactionPalette[0].color);
            break;
          case DisplayMode.CMVChange:
            colorValues = new List<ColorValue>();
            for (int i = 1; i < compactionPalette.Count - 1; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                compactionPalette[i].value));
            }
            cmvPercentChangePalette = DetailPalette.CreateDetailPalette(colorValues,
              compactionPalette[compactionPalette.Count - 1].color, compactionPalette[0].color);
            break;
        }

      }
      return CompactionColorPalettesResult.CreateCompactionColorPalettesResult(
        cmvDetailPalette, passCountDetailPalette, passCountSummaryPalette, cutFillPalette, temperatureSummaryPalette,
        cmvSummaryPalette, mdpSummaryPalette, cmvPercentChangePalette, speedSummaryPalette, temperatureDetailPalette);
    }

    /// <summary>
    /// Get the elevation color palette for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <returns>Elevation color palette</returns>
    [ProjectUidVerifier]
    [Route("api/v2/elevationpalette")]
    [HttpGet]
    public async Task<CompactionDetailPaletteResult> GetElevationPalette(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation("GetElevationPalette: " + Request.QueryString);

      var projectSettings = await this.GetProjectSettingsTargets(projectUid);
      var projectSettingsColors = await this.GetProjectSettingsColors(projectUid);

      var filter = await GetCompactionFilter(projectUid, filterUid);

      ElevationStatisticsResult elevExtents = elevProxy.GetElevationRange(GetLegacyProjectId(projectUid), filter, projectSettings);
      var compactionPalette = this.SettingsManager.CompactionPalette(DisplayMode.Height, elevExtents, projectSettings, projectSettingsColors);

      DetailPalette elevationPalette = null;
      if (compactionPalette != null)
      {
        List<ColorValue> colorValues = new List<ColorValue>();
        for (int i = 1; i < compactionPalette.Count - 1; i++)
        {
          colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
            compactionPalette[i].value));
        }
        elevationPalette = DetailPalette.CreateDetailPalette(colorValues,
          compactionPalette[compactionPalette.Count - 1].color, compactionPalette[0].color);
      }

      return CompactionDetailPaletteResult.CreateCompactionDetailPaletteResult(elevationPalette);
    }
  }
}