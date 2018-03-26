using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICVolumeCalculationsDecls;
using System;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

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
    ///   Processes the xxx request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">Request to process</param>
    /// <returns>a xxxResult if successful</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      TileRequest request = item as TileRequest;

      try
      {
        RaptorConverters.convertGridOrLLBoundingBox(request.boundBoxGrid, request.boundBoxLL, out TWGS84Point bl, out TWGS84Point tr,
  out bool coordsAreGrid);
        TICFilterSettings filter1 =
          RaptorConverters.ConvertFilter(request.filterId1, request.filter1, request.projectId);
        TICFilterSettings filter2 =
          RaptorConverters.ConvertFilter(request.filterId2, request.filter2, request.projectId);
        TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.computeVolType);
        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
          RaptorConverters.AdjustFilterToFilter(filter1, filter2);

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.mode, request.computeVolType);

        TASNodeErrorStatus raptorResult = raptorClient.GetRenderedMapTileWithRepresentColor
(request.projectId ?? -1,
  ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
    TASNodeCancellationDescriptorType.cdtWMSTile),
  RaptorConverters.convertDisplayMode(request.mode),
  RaptorConverters.convertColorPalettes(request.palettes, request.mode),
  bl, tr,
  coordsAreGrid,
  request.width,
  request.height,
  filter1,
  filter2,
  RaptorConverters.convertOptions(null, request.liftBuildSettings, request.computeVolNoChangeTolerance,
    request.filterLayerMethod, request.mode, request.setSummaryDataLayersVisibility),
  RaptorConverters.DesignDescriptor(request.designDescriptor),
  volType,
  request.representationalDisplayColor,
  out MemoryStream tile);

        log.LogTrace($"Received {raptorResult} as a result of execution and tile is {tile == null}");

        if ((raptorResult == TASNodeErrorStatus.asneOK) ||
            (raptorResult == TASNodeErrorStatus.asneInvalidCoordinateRange))
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
            $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int) raptorResult)}.");

          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
            ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get requested tile with error: {ContractExecutionStates.FirstNameWithOffset((int) raptorResult)}."));
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