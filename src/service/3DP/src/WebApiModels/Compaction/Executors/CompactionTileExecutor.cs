using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using SVOICVolumeCalculationsDecls;
#endif
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Log4NetExtensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// V2 Tile executor. Same as V1 but without the reconcileTopFilterAndVolumeComputationMode as this is done externally.
  /// </summary>
  public class CompactionTileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionTileExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the request for type of T.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TileRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
        {
#endif
          var trexRequest = new TRexTileRequest(
            request.ProjectUid.Value,
            request.Mode,
            request.Palettes,
            request.DesignDescriptor,
            request.Filter1,
            request.Filter2,
            request.BoundBoxLatLon,
            request.BoundBoxGrid,
            request.Width,
            request.Height,
            AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings)
          );
          var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(trexRequest, "/tile", customHeaders);

          using (var ms = new MemoryStream())
          {
            fileResult.CopyTo(ms);
            return new TileResult(ms.ToArray());
          }
#if RAPTOR
        }

        return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(TileRequest request)
    {
      RaptorConverters.convertGridOrLLBoundingBox(request.BoundBoxGrid, request.BoundBoxLatLon, out var bottomLeftPoint, out var topRightPoint,
        out bool coordsAreGrid);

      var baseFilter = RaptorConverters.ConvertFilter(request.Filter1, request.ProjectId, raptorClient);
      var topFilter = RaptorConverters.ConvertFilter(request.Filter2, request.ProjectId, raptorClient);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);

      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolumesType);
      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters && !request.ExplicitFilters)
      {
        RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
      }

      if ((baseFilter == null || topFilter == null) && designDescriptor.IsNull() ||
           baseFilter == null && topFilter == null)
      {
        throw new ServiceException(
          HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Invalid surface configuration."));
      }

      // Fix for Raptor issue where the 'below' color is not being set correctly and the Red and Blue parts of the RGB are being
      // flipped incorrectly (Delphi uses BGR not RGB like C#). 
      // The following is a workaround to this behaviour and WILL be removed once the Raptor behaviour is properly understood (maybe fixed).
      if (request.Mode == DisplayMode.Design3D || request.Mode == DisplayMode.Height)
      {
        byte[] values = BitConverter.GetBytes(request.Palettes[0].Color);

        // Flip the bits represending Red and Blue in the color byte. We don't care about endian differences here; assume isLittleEndian=true.
        var rgbRed = values[0];
        values[0] = values[2];
        values[2] = rgbRed;

        request.Palettes[0].Color = BitConverter.ToUInt32(values, 0);
      }
      // End temporary fix.

      var raptorResult = raptorClient.GetRenderedMapTileWithRepresentColor(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtWMSTile),
        RaptorConverters.convertDisplayMode(request.Mode),
        RaptorConverters.convertColorPalettes(request.Palettes, request.Mode),
        bottomLeftPoint, topRightPoint,
        coordsAreGrid,
        request.Width,
        request.Height,
        baseFilter,
        topFilter,
        RaptorConverters.convertOptions(null, request.LiftBuildSettings, request.ComputeVolNoChangeTolerance,
          request.FilterLayerMethod, request.Mode, request.SetSummaryDataLayersVisibility),
        designDescriptor,
        volType,
        request.RepresentationalDisplayColor,
        out MemoryStream tile);

      if (log.IsTraceEnabled())
        log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

      if (raptorResult == TASNodeErrorStatus.asneOK ||
          raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
      {
        if (tile != null)
          return ConvertResult(tile, raptorResult);
        else
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null tile returned");
      }
      if (log.IsTraceEnabled())
        log.LogTrace(
          $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}.");

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
        ContractExecutionStatesEnum.InternalProcessingError,
        $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}."));
    }

    private TileResult ConvertResult(MemoryStream tile, TASNodeErrorStatus raptorResult)
    {
      log.LogDebug("Raptor result for Tile: " + raptorResult);
      return new TileResult(tile.ToArray(), raptorResult != TASNodeErrorStatus.asneOK);
    }
#endif

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
