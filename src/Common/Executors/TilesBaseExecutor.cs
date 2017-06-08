using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICVolumeCalculationsDecls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace Common.Executors
{
  public abstract class TilesBaseExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public TilesBaseExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
      // ...
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TilesBaseExecutor()
    {
      // ...
    }

    /// <summary>
    ///   Processes the xxx request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">Request to process</param>
    /// <returns>a xxxResult if successful</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      TileRequest request = GetRequest(item);

      try
      {
        TWGS84Point bl, tr;
        bool coordsAreGrid;
        RaptorConverters.convertGridOrLLBoundingBox(request.boundBoxGrid, request.boundBoxLL, out bl, out tr,
          out coordsAreGrid);
        TICFilterSettings filter1 =
          RaptorConverters.ConvertFilter(request.filterId1, request.filter1, request.projectId);
        TICFilterSettings filter2 =
          RaptorConverters.ConvertFilter(request.filterId2, request.filter2, request.projectId);
        TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.computeVolType);
        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
          RaptorConverters.AdjustFilterToFilter(filter1, filter2);

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.mode, request.computeVolType);

        MemoryStream tile;
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
          out tile);

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
            String.Format("Failed to get requested tile with error: {0}.",
              ContractExecutionStates.FirstNameWithOffset((int)raptorResult)));

          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(
            ContractExecutionStatesEnum.FailedToGetResults,
            String.Format("Failed to get requested tile with error: {0}.",
              ContractExecutionStates.FirstNameWithOffset((int)raptorResult))));
        }

      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    protected abstract TileRequest GetRequest(object item);

    private TileResult ConvertResult(MemoryStream tile, TASNodeErrorStatus raptorResult)
    {
      log.LogDebug("Raptor result for Tile: " + raptorResult);
      return TileResult.CreateTileResult(tile.ToArray(), raptorResult);
    }

    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
