using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.MasterDataProxies;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes;
using VSS.Productivity3D.WebApiModels.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;
using ColorValue = VSS.Productivity3D.WebApiModels.Compaction.Models.Palettes.ColorValue;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting color palettes for displaying Raptor production data
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionPaletteController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// 
    /// </summary>
    private readonly IASNodeClient raptorClient;
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="elevProxy">Elevation extents proxy</param>
    public CompactionPaletteController(IASNodeClient raptorClient, ILoggerFactory logger, IFileListProxy fileListProxy, IElevationExtentsProxy elevProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<CompactionPaletteController>();
      this.fileListProxy = fileListProxy;
      this.elevProxy = elevProxy;
    }

    /// <summary>
    /// Get color palettes for a project.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <returns>Color palettes for all display types</returns>
    [Route("api/v2/compaction/colorpalettes")]
    [HttpGet]
    public async Task<CompactionColorPalettesResult> GetColorPalettes(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid)
    {
      log.LogInformation("GetColorPalettes: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      List<DisplayMode> modes = new List<DisplayMode>
      {
        DisplayMode.Height,
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

      DetailPalette elevationPalette = null;
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
        ElevationStatisticsResult elevExtents = mode == DisplayMode.Height
          ? elevProxy.GetElevationRange(raptorClient, projectId.Value, null)
          : null;
        var compactionPalette = CompactionSettings.CompactionPalette(mode, elevExtents);
        switch (mode)
        {
          case DisplayMode.Height:
            if (compactionPalette != null)
            {
              colorValues = new List<ColorValue>();
              for (int i = 1; i < compactionPalette.Count - 1; i++)
              {
                colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                  compactionPalette[i].value));
              }
              elevationPalette = DetailPalette.CreateDetailPalette(colorValues,
                compactionPalette[compactionPalette.Count - 1].color, compactionPalette[0].color);
            }
            break;
          case DisplayMode.CCV:
            colorValues = new List<ColorValue>();
            for (int i = 0; i < compactionPalette.Count; i++)
            {
              colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                compactionPalette[i].value));
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
        elevationPalette, cmvDetailPalette, passCountDetailPalette, passCountSummaryPalette, cutFillPalette,
        temperatureSummaryPalette,
        cmvSummaryPalette, mdpSummaryPalette, cmvPercentChangePalette, speedSummaryPalette,
        temperatureDetailPalette);
    }

    /// <summary>
    /// Get the elevation color palette for a project.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber">The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>    
    /// <returns>Elevation color palette</returns>
    [Route("api/v2/compaction/elevationpalette")]
    [HttpGet]
    public async Task<CompactionDetailPaletteResult> GetElevationPalette(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetElevationPalette: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }

      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, 
        Request.Headers.GetCustomHeaders());
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);
      ElevationStatisticsResult elevExtents = elevProxy.GetElevationRange(raptorClient, projectId.Value, filter);
      var compactionPalette = CompactionSettings.CompactionPalette(DisplayMode.Height, elevExtents);

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
