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
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionPatchV2Executor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionPatchV2Executor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      var request = item as PatchRequest;

      // Note: The numPatches out parameter is ignored in favour of the same value returned in the PatchResult proper. This will be removed
      // in due course once the breaking modifications process is agreed with BC.
      try
      {
        var filter1 = RaptorConverters.ConvertFilter(request.FilterId1, request.Filter1, request.ProjectId);
        var filter2 = RaptorConverters.ConvertFilter(request.FilterId2, request.Filter2, request.ProjectId);
        var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolType);

        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
        {
          RaptorConverters.AdjustFilterToFilter(ref filter1, filter2);
        }

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.Mode, request.ComputeVolType);

        var raptorResult = raptorClient.RequestDataPatchPage(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtDataPatches),
          RaptorConverters.convertDisplayMode(request.Mode),
          RaptorConverters.convertColorPalettes(request.Palettes, request.Mode),
          request.RenderColorValues,
          filter1,
          filter2,
          RaptorConverters.convertOptions(null, request.LiftBuildSettings,
                  request.ComputeVolNoChangeTolerance, request.FilterLayerMethod, request.Mode, request.setSummaryDataLayersVisibility),
          RaptorConverters.DesignDescriptor(request.DesignDescriptor),
          volType,
          request.PatchNumber,
          request.PatchSize,
          out var patch,
          out _);

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = patch != null
            ? ConvertPatchResult(patch.ToArray())
            : new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null patch returned");
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested patch with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}."));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    private PatchResult ConvertPatchResult(byte[] patch)
    {
      using (var ms = new MemoryStream(patch))
      {
        var totalNumPatchesRequired = StreamUtils.__Global.ReadIntegerFromStream(ms);
        _ = StreamUtils.__Global.ReadBooleanFromStream(ms); // Discard valuesRenderedToColors flag, it's not needed for this response.
        var numSubgridsInPatch = StreamUtils.__Global.ReadIntegerFromStream(ms);
        double cellSize = StreamUtils.__Global.ReadDateTimeFromStream(ms);
        var subgrids = new PatchSubgridResultBase[numSubgridsInPatch];

        // From Raptor: 1 << ((FNumLevels * kSubGridIndexBitsPerLevel) -1)
        const int indexOriginOffset = 1 << 29;

        for (var i = 0; i < numSubgridsInPatch; i++)
        {
          var worldOriginX = (StreamUtils.__Global.ReadIntegerFromStream(ms) - indexOriginOffset) * cellSize;
          var worldOriginY = (StreamUtils.__Global.ReadIntegerFromStream(ms) - indexOriginOffset) * cellSize;
          var isNull = StreamUtils.__Global.ReadBooleanFromStream(ms);

          log.LogDebug($"Subgrid {i + 1} in patch has world origin of {worldOriginX}:{worldOriginY}. IsNull?:{isNull}");

          float elevationOrigin = 0;

          // Protobuf is limited to single dimension arrays, so we cannot use the normal [32,32] layout type used by other patch executors.
          PatchCellHeightResult[] cells = null;

          if (!isNull)
          {
            elevationOrigin = StreamUtils.__Global.ReadSingleFromStream(ms);

            log.LogDebug($"Subgrid elevation origin in {elevationOrigin}");

            const int arrayLength = 32 * 32;
            cells = new PatchCellHeightResult[arrayLength];

            for (var j = 0; j < arrayLength; j++)
            {
              var elevOffset = StreamUtils.__Global.ReadWordFromStream(ms);
              
              // Return raw elevation offset delta and the client will determine the elevation using the following equation:
              // float elevation = elevOffset != 0xffff ? (float)(elevationOrigin + (elevOffset / 1000.0)) : -100000;

              // Increment all non-null values by 1. The consumer will subtract 1 from all non zero values to determine the true elevation offset.
              // The goal is to return the smallest value possible, so we leverage the variant lenght feature of Protobuf to use the smallest number
              // of bytes possible for each cell element.
              ushort elevation = (ushort) (elevOffset != 0xffff ? elevOffset + 0x1 : 0x0);

              cells[j] = PatchCellHeightResult.Create(elevation);
            }
          }

          subgrids[i] = PatchSubgridOriginProtobufResult.Create(worldOriginX, worldOriginY, isNull, elevationOrigin, cells);
        }

        return PatchResult.Create(cellSize,
          numSubgridsInPatch,
          totalNumPatchesRequired,
          subgrids);
      }
    }
  }
}
