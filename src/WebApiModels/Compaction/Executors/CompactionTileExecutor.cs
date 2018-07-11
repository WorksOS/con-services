using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICVolumeCalculationsDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;

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
      ContractExecutionResult result;
      TileRequest request = item as TileRequest;

      try
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
            request.FilterLayerMethod, request.Mode, request.setSummaryDataLayersVisibility),
          designDescriptor,
          volType,
          request.RepresentationalDisplayColor,
          out MemoryStream tile);

        log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

        if (raptorResult == TASNodeErrorStatus.asneOK ||
            raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange)
        {
          if (tile != null)
          {
            result = ConvertResult(tile, raptorResult);
          }
          else
          {
            result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Null tile returned");
          }
        }
        else
        {
          log.LogTrace(
            $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}.");

          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
            ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}."));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    private TileResult ConvertResult(MemoryStream tile, TASNodeErrorStatus raptorResult)
    {
      log.LogDebug("Raptor result for Tile: " + raptorResult);
      return TileResult.CreateTileResult(tile.ToArray(), raptorResult);
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
