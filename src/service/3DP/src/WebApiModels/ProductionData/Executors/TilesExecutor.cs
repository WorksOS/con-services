using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using SVOICVolumeCalculationsDecls;
using VSS.Productivity3D.Common.Proxies;
#endif
using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TileRequest>(item);
#if RAPTOR
        if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_TILES") ?? false)
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
      RaptorConverters.convertGridOrLLBoundingBox(
        request.BoundBoxGrid, request.BoundBoxLatLon, out var bl, out var tr, out bool coordsAreGrid);

      var filter1 = RaptorConverters.ConvertFilter(request.Filter1, request.ProjectId, raptorClient);
      var filter2 = RaptorConverters.ConvertFilter(request.Filter2, request.ProjectId, raptorClient);
      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolumesType);

      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
        RaptorConverters.AdjustFilterToFilter(ref filter1, filter2);
      }

      RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.Mode, request.ComputeVolumesType);

      var raptorResult = raptorClient.GetRenderedMapTileWithRepresentColor(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
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

      if (log.IsTraceEnabled())
        log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

      if (raptorResult == TASNodeErrorStatus.asneOK ||
          raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
      {
        if (tile != null)
          return ConvertResult(tile, raptorResult);

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
