using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Common.Utilities;
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
          new DesignOffset(request.DesignDescriptor?.FileUid ?? Guid.Empty, request.DesignDescriptor.Offset)
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

          if (request.Palettes != null)
          {
            ccaSummaryPalette.UndercompactedColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            ccaSummaryPalette.CompactedColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            ccaSummaryPalette.OvercompactedColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          break;
        case DisplayMode.CCV:
          convertedPalette = new CMVPalette();

          var cmvPalette = ((CMVPalette)convertedPalette);

          cmvPalette.CMVPercentageRange.Min = request.LiftBuildSettings.CCVRange?.Min ?? PERCENTAGE_RANGE_MIN;
          cmvPalette.CMVPercentageRange.Max = request.LiftBuildSettings.CCVRange?.Max ?? PERCENTAGE_RANGE_MAX;

          cmvPalette.UseMachineTargetCMV = !request.LiftBuildSettings.OverridingMachineCCV.HasValue;
          cmvPalette.AbsoluteTargetCMV = request.LiftBuildSettings.OverridingMachineCCV ?? 0;

          cmvPalette.TargetCCVColour = Color.Green;
          cmvPalette.DefaultDecoupledCMVColour = Color.Black;
          break;
        case DisplayMode.CCVPercentSummary:
          convertedPalette = new CMVSummaryPalette();

          var cmvSummaryPalette = ((CMVSummaryPalette) convertedPalette);

          cmvSummaryPalette.CMVPercentageRange.Min = request.LiftBuildSettings.CCVRange?.Min ?? PERCENTAGE_RANGE_MIN;
          cmvSummaryPalette.CMVPercentageRange.Max = request.LiftBuildSettings.CCVRange?.Max ?? PERCENTAGE_RANGE_MAX;

          cmvSummaryPalette.UseMachineTargetCMV = !request.LiftBuildSettings.OverridingMachineCCV.HasValue;
          cmvSummaryPalette.AbsoluteTargetCMV = request.LiftBuildSettings.OverridingMachineCCV ?? 0;

          if (request.Palettes != null)
          {
            cmvSummaryPalette.AboveCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            cmvSummaryPalette.WithinCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            cmvSummaryPalette.BelowCMVTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          break;
        case DisplayMode.CutFill:
          convertedPalette = new CutFillPalette();
          break;
        case DisplayMode.Height:
          var extent = siteModel.GetAdjustedDataModelSpatialExtents(new Guid[0]);

          convertedPalette = new HeightPalette(extent.MinZ, extent.MaxZ);

          if (request.Palettes != null)
          {

            var colors = new Color[request.Palettes.Count];

            for (var i = 0; i < request.Palettes.Count; i++)
              colors[i] = ColorUtility.UIntToColor(request.Palettes[i].Color);

            ((HeightPalette) convertedPalette).ElevationPalette = colors;
          }

          break;
        case DisplayMode.MDP:
          convertedPalette = new MDPPalette();

          var mdpPalette = ((MDPPalette)convertedPalette);

          mdpPalette.MDPPercentageRange.Min = request.LiftBuildSettings.MDPRange?.Min ?? PERCENTAGE_RANGE_MIN;
          mdpPalette.MDPPercentageRange.Max = request.LiftBuildSettings.MDPRange?.Max ?? PERCENTAGE_RANGE_MAX;

          mdpPalette.UseMachineTargetMDP = !request.LiftBuildSettings.OverridingMachineMDP.HasValue;
          mdpPalette.AbsoluteTargetMDP = request.LiftBuildSettings.OverridingMachineMDP ?? 0;

          mdpPalette.TargetMDPColour = Color.Green;
          break;
        case DisplayMode.MDPPercentSummary:
          convertedPalette = new MDPSummaryPalette();

          var mdpSummaryPalette = ((MDPSummaryPalette)convertedPalette);

          mdpSummaryPalette.MDPPercentageRange.Min = request.LiftBuildSettings.MDPRange?.Min ?? PERCENTAGE_RANGE_MIN;
          mdpSummaryPalette.MDPPercentageRange.Max = request.LiftBuildSettings.MDPRange?.Max ?? PERCENTAGE_RANGE_MAX;

          mdpSummaryPalette.UseMachineTargetMDP = !request.LiftBuildSettings.OverridingMachineMDP.HasValue;
          mdpSummaryPalette.AbsoluteTargetMDP = request.LiftBuildSettings.OverridingMachineMDP ?? 0;

          if (request.Palettes != null)
          {
            mdpSummaryPalette.AboveMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            mdpSummaryPalette.WithinMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            mdpSummaryPalette.BelowMDPTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          break;
        case DisplayMode.PassCount:
          convertedPalette = new PassCountPalette();
          break;
        case DisplayMode.PassCountSummary:
          convertedPalette = new PassCountSummaryPalette();

          var passCountPalette = ((PassCountSummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            passCountPalette.AbovePassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            passCountPalette.WithinPassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            passCountPalette.BelowPassTargetRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

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

          if (request.Palettes != null)
          {
            speedSummaryPalette.OverSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            speedSummaryPalette.WithinSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            speedSummaryPalette.LowerSpeedRangeColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          speedSummaryPalette.MachineSpeedTarget.Min = request.LiftBuildSettings.MachineSpeedTarget?.MinTargetMachineSpeed ?? CellPassConsts.NullMachineSpeed;
          speedSummaryPalette.MachineSpeedTarget.Max = request.LiftBuildSettings.MachineSpeedTarget?.MaxTargetMachineSpeed ?? CellPassConsts.NullMachineSpeed;
          break;
        case DisplayMode.TemperatureDetail:
          convertedPalette = new TemperaturePalette();
          break;
        case DisplayMode.TemperatureSummary:
          convertedPalette = new TemperatureSummaryPalette();

          var temperatureSummaryPalette = ((TemperatureSummaryPalette)convertedPalette);

          if (request.Palettes != null)
          {
            temperatureSummaryPalette.AboveMaxLevelColour = ColorUtility.UIntToColor(request.Palettes[0].Color);
            temperatureSummaryPalette.WithinLevelsColour = ColorUtility.UIntToColor(request.Palettes[1].Color);
            temperatureSummaryPalette.BelowMinLevelColour = ColorUtility.UIntToColor(request.Palettes[2].Color);
          }

          temperatureSummaryPalette.UseMachineTempWarningLevels = request.LiftBuildSettings.OverridingTemperatureWarningLevels == null;
          temperatureSummaryPalette.TemperatureLevels.Min = request.LiftBuildSettings.OverridingTemperatureWarningLevels?.Min ?? TEMPERATURE_LEVELS_MIN;
          temperatureSummaryPalette.TemperatureLevels.Max = request.LiftBuildSettings.OverridingTemperatureWarningLevels?.Max ?? TEMPERATURE_LEVELS_MAX;
          break;
        default:
          throw new TRexException($"No implemented colour palette for this mode ({request.Mode})");
      }

      if (request.Mode != DisplayMode.Height &&
          request.Mode != DisplayMode.CCVPercentSummary &&
          request.Mode != DisplayMode.PassCountSummary && 
          request.Mode != DisplayMode.CCASummary &&
          request.Mode != DisplayMode.MDPPercentSummary &&
          request.Mode != DisplayMode.TargetSpeedSummary &&
          request.Mode != DisplayMode.TemperatureSummary)
      {
        if (request.Palettes != null)
        {
          var transitions = new Transition[request.Palettes.Count];

        for (var i = 0; i < request.Palettes.Count; i++)
          transitions[i] = new Transition(request.Palettes[i].Value, ColorUtility.UIntToColor(request.Palettes[i].Color));

        convertedPalette.PaletteTransitions = transitions;
        }
      }

      return convertedPalette;
    }
  }
}
