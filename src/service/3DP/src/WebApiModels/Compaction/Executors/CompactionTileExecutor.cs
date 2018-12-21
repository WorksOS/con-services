using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICVolumeCalculationsDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as TileRequest;

        if (request == null)
          ThrowRequestTypeCastException<TileRequest>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_TILES"), out var useTrexGateway);

        if (useTrexGateway)
        {
          var fileResult = trexCompactionDataProxy.SendProductionDataTileRequest(request, customHeaders).Result;

          using (var ms = new MemoryStream())
          {
            fileResult.CopyTo(ms);
            return new TileResult(ms.ToArray());
          }
        }

        return ProcessWithRaptor(request);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private ContractExecutionResult ProcessWithRaptor(TileRequest request)
    {
      RaptorConverters.convertGridOrLLBoundingBox(request.BoundBoxGrid, request.BoundBoxLatLon, out var bottomLeftPoint, out var topRightPoint,
        out bool coordsAreGrid);

      var baseFilter = RaptorConverters.ConvertFilter(request.FilterId1, request.Filter1, request.ProjectId);
      var topFilter = RaptorConverters.ConvertFilter(request.FilterId2, request.Filter2, request.ProjectId);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);

      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolumesType);
      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
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
        request.ProjectId ?? -1,
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

      log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

      if (raptorResult == TASNodeErrorStatus.asneOK ||
          raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
      {
        if (tile != null)
          return ConvertResult(tile, raptorResult);
        else
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null tile returned");
      }

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

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
