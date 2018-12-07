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
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.Common.Executors
{
  public class TilesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public TilesExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the request for type T.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TileRequest>(item);

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
      RaptorConverters.convertGridOrLLBoundingBox(
        request.BoundBoxGrid, request.BoundBoxLatLon, out var bl, out var tr, out bool coordsAreGrid);

      var filter1 = RaptorConverters.ConvertFilter(request.FilterId1, request.Filter1, request.ProjectId);
      var filter2 = RaptorConverters.ConvertFilter(request.FilterId2, request.Filter2, request.ProjectId);
      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolumesType);

      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
        RaptorConverters.AdjustFilterToFilter(ref filter1, filter2);
      }

      RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.Mode, request.ComputeVolumesType);

      var raptorResult = raptorClient.GetRenderedMapTileWithRepresentColor(
        request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtWMSTile),
        RaptorConverters.convertDisplayMode(request.Mode),
        RaptorConverters.convertColorPalettes(request.Palettes, request.Mode),
        bl, tr,
        coordsAreGrid,
        request.Width,
        request.Height,
        filter1,
        filter2,
        RaptorConverters.convertOptions(null, request.LiftBuildSettings, request.ComputeVolNoChangeTolerance,
          request.FilterLayerMethod, request.Mode, request.SetSummaryDataLayersVisibility),
        RaptorConverters.DesignDescriptor(request.DesignDescriptor),
        volType,
        request.RepresentationalDisplayColor,
        out var tile);

      log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

      if (raptorResult == TASNodeErrorStatus.asneOK ||
          raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
      {
        if (tile != null)
          return ConvertResult(tile, raptorResult);

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
