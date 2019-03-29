using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class TileExecutor : BaseExecutor
  {
    public TileExecutor(IConfigurationStore configStore, ILoggerFactory logger, 
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TileExecutor()
    {
    }

    private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel, bool includeSurveyedSurfaces)
    {
      return siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TileRequest;

      if (request == null)
        ThrowRequestTypeCastException<TileRequest>();

      BoundingWorldExtent3D extents = null;
      bool hasGridCoords = false;
      if (request.BoundBoxLatLon != null)
      {
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DLatLon, BoundingWorldExtent3D>(request.BoundBoxLatLon);
      }
      else if (request.BoundBoxGrid != null)
      {
        hasGridCoords = true;
        extents = AutoMapperUtility.Automapper.Map<BoundingBox2DGrid, BoundingWorldExtent3D>(request.BoundBoxGrid);
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      
      var tileRequest = new TileRenderRequest();
      var response = tileRequest.Execute(
        new TileRenderRequestArgument
        (siteModel.ID,
          request.Mode,
          ConvertColorPalettes(request, siteModel),
          extents,
          hasGridCoords,
          request.Width, // PixelsX
          request.Height, // PixelsY
          new FilterSet(ConvertFilter(request.Filter1, siteModel), ConvertFilter(request.Filter2, siteModel)),
          request.DesignDescriptor?.FileUid ?? Guid.Empty
        )) as TileRenderResponse_Core2;

      return new TileResult(response?.TileBitmapData);
    }

    /// <summary>
    /// Processes the tile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    private PaletteBase ConvertColorPalettes(TileRequest request, ISiteModel siteModel)
    {
      const double PERCENTAGE_RANGE_MIN = 0.0;
      const double PERCENTAGE_RANGE_MAX = 100.0;
      const ushort PASS_COUNT_TARGET_RANGE_MIN = 1;
      const ushort PASS_COUNT_TARGET_RANGE_MAX = ushort.MaxValue;
      const ushort TEMPERATURE_LEVELS_MIN = 0;
      const ushort TEMPERATURE_LEVELS_MAX = 100;

      PaletteBase convertedPalette;

      switch (request.Mode)
      {
        case DisplayMode.CCA:
          convertedPalette = new CCAPalette();
          break;
        case DisplayMode.CCASummary:
          convertedPalette = new CCASummaryPalette();

          var ccaSummaryPalette = ((CCASummaryPalette)convertedPalette);

          ccaSummaryPalette.UndercompactedColour = Color.FromArgb((int)request.Palettes[0].Color);
          ccaSummaryPalette.CompactedColour = Color.FromArgb((int)request.Palettes[1].Color);
          ccaSummaryPalette.OvercompactedColour = Color.FromArgb((int)request.Palettes[2].Color);
          break;
        case DisplayMode.CCV:
        case DisplayMode.CCVSummary:
          convertedPalette = new CMVPalette();

          var cmvPalette = ((CMVPalette) convertedPalette);

          cmvPalette.CMVPercentageRange.Min = request.LiftBuildSettings.CCVRange?.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPalette.CMVPercentageRange.Max = request.LiftBuildSettings.CCVRange?.Max ?? PERCENTAGE_RANGE_MAX;
          cmvPalette.TargetCCVColour = Color.Green;
          break;
        case DisplayMode.CutFill:
          convertedPalette = new CutFillPalette();
          break;
        case DisplayMode.Height:
          BoundingWorldExtent3D extent = siteModel.GetAdjustedDataModelSpatialExtents(new Guid[0]);

          convertedPalette = new HeightPalette(extent.MinZ, extent.MaxZ);

          var colors = new Color[request.Palettes.Count];

          for (var i = 0; i < request.Palettes.Count; i++)
            colors[i] = Color.FromArgb((int) request.Palettes[i].Color);

          ((HeightPalette)convertedPalette).ElevationPalette = colors;
          break;
        case DisplayMode.MDPPercentSummary:
          convertedPalette = new MDPSummaryPalette();

          var mdpPalette = ((MDPSummaryPalette)convertedPalette);

          mdpPalette.MDPPercentageRange.Min = request.LiftBuildSettings.MDPRange?.Min ?? PERCENTAGE_RANGE_MIN;
          mdpPalette.MDPPercentageRange.Max = request.LiftBuildSettings.MDPRange?.Max ?? PERCENTAGE_RANGE_MAX;
          mdpPalette.TargetMDPColour = Color.Green;
          mdpPalette.UseMachineTargetMDP = !request.LiftBuildSettings.OverridingMachineMDP.HasValue;
          mdpPalette.AbsoluteTargetMDP = request.LiftBuildSettings.OverridingMachineMDP ?? 0; 
          break;
        case DisplayMode.PassCount:
          convertedPalette = new PassCountPalette();
          break;
        case DisplayMode.PassCountSummary:
          convertedPalette = new PassCountSummaryPalette();

          var passCountPalette = ((PassCountSummaryPalette)convertedPalette);

          passCountPalette.BelowPassTargetRangeColour = Color.FromArgb((int) request.Palettes[0].Color);
          passCountPalette.AbovePassTargetRangeColour = Color.FromArgb((int)request.Palettes[1].Color);
          passCountPalette.AbovePassTargetRangeColour = Color.FromArgb((int)request.Palettes[2].Color);
          passCountPalette.UseMachineTargetPass = request.LiftBuildSettings.OverridingTargetPassCountRange == null;
          passCountPalette.TargetPassCountRange.Min = request.LiftBuildSettings.OverridingTargetPassCountRange?.Min ?? PASS_COUNT_TARGET_RANGE_MIN;
          passCountPalette.TargetPassCountRange.Max = request.LiftBuildSettings.OverridingTargetPassCountRange?.Max ?? PASS_COUNT_TARGET_RANGE_MAX;
          break;
        case DisplayMode.MachineSpeed:
          convertedPalette = new SpeedPalette();
          break;
        case DisplayMode.TargetSpeedSummary:
          convertedPalette = new SpeedSummaryPalette();

          var speedSummaryPalette = ((SpeedSummaryPalette)convertedPalette);

          speedSummaryPalette.LowerSpeedRangeColour = Color.FromArgb((int)request.Palettes[0].Color);
          speedSummaryPalette.WithinSpeedRangeColour = Color.FromArgb((int)request.Palettes[1].Color);
          speedSummaryPalette.OverSpeedRangeColour = Color.FromArgb((int)request.Palettes[2].Color);
          speedSummaryPalette.MachineSpeedTarget.Min = request.LiftBuildSettings.MachineSpeedTarget?.MinTargetMachineSpeed ?? CellPassConsts.NullMachineSpeed;
          speedSummaryPalette.MachineSpeedTarget.Max = request.LiftBuildSettings.MachineSpeedTarget?.MaxTargetMachineSpeed ?? CellPassConsts.NullMachineSpeed;
          break;
        case DisplayMode.TemperatureDetail:
          convertedPalette = new TemperaturePalette();
          break;
        case DisplayMode.TemperatureSummary:
          convertedPalette = new TemperatureSummaryPalette();

          var temperatureSummaryPalette = ((TemperatureSummaryPalette)convertedPalette);

          temperatureSummaryPalette.BelowMinLevelColour = Color.FromArgb((int)request.Palettes[0].Color);
          temperatureSummaryPalette.WithinLevelsColour = Color.FromArgb((int)request.Palettes[1].Color);
          temperatureSummaryPalette.AboveMaxLevelColour = Color.FromArgb((int)request.Palettes[2].Color);
          temperatureSummaryPalette.UseMachineTempWarningLevels = request.LiftBuildSettings.OverridingTemperatureWarningLevels == null;
          temperatureSummaryPalette.TemperatureLevels.Min = request.LiftBuildSettings.OverridingTemperatureWarningLevels?.Min ?? TEMPERATURE_LEVELS_MIN;
          temperatureSummaryPalette.TemperatureLevels.Max = request.LiftBuildSettings.OverridingTemperatureWarningLevels?.Max ?? TEMPERATURE_LEVELS_MAX;
          break;
        default:
          throw new TRexException($"No implemented colour palette for this mode ({request.Mode})");
      }

      if (request.Mode != DisplayMode.Height && 
          request.Mode != DisplayMode.PassCountSummary && 
          request.Mode != DisplayMode.CCASummary &&
          request.Mode != DisplayMode.MDPSummary &&
          request.Mode != DisplayMode.TargetSpeedSummary &&
          request.Mode != DisplayMode.TemperatureSummary)
      {
        var transitions = new Transition[request.Palettes.Count];

        for (var i = 0; i < request.Palettes.Count; i++)
          transitions[i] = new Transition(request.Palettes[i].Value, Color.FromArgb((int)request.Palettes[i].Color));

        convertedPalette.PaletteTransitions = transitions;
      }

      return convertedPalette;
    }
  }
}
