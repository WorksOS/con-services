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

      if (request == null)
        ThrowRequestTypeCastException<PatchRequest>();

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

        var raptorResult = raptorClient.RequestDataPatchPageWithTime(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtDataPatches),
          RaptorConverters.convertDisplayMode(request.Mode),
          filter1,
          filter2,
          RaptorConverters.DesignDescriptor(request.DesignDescriptor),
          volType,
          RaptorConverters.convertOptions(null, request.LiftBuildSettings,
            request.ComputeVolNoChangeTolerance, request.FilterLayerMethod, request.Mode, request.SetSummaryDataLayersVisibility),
          request.PatchNumber,
          request.PatchSize,
          out var patch,
          out _);

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = patch != null
            ? ConvertPatchResult(patch.ToArray(), request.IncludeTimeOffsets)
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

    private PatchResult ConvertPatchResult(byte[] patch, bool includeTimeOffsets)
    {
      using (var ms = new MemoryStream(patch))
      {
        var numSubgridsInPatch = StreamUtils.__Global.ReadIntegerFromStream(ms);
        var dataPatchSubgridCount = StreamUtils.__Global.ReadIntegerFromStream(ms);
        double cellSize = StreamUtils.__Global.ReadDoubleFromStream(ms);

        var subgrids = new PatchSubgridResultBase[dataPatchSubgridCount];

        for (var i = 0; i < dataPatchSubgridCount; i++)
        {
          var subgridOriginX = StreamUtils.__Global.ReadDoubleFromStream(ms);
          var subgridOriginY = StreamUtils.__Global.ReadDoubleFromStream(ms);
          var isNull = StreamUtils.__Global.ReadBooleanFromStream(ms);

          log.LogDebug($"Subgrid {i + 1} in patch has world origin of {subgridOriginX}:{subgridOriginY}. IsNull?:{isNull}");

          if (isNull)
          {
            continue;
          }

          float elevationOrigin = StreamUtils.__Global.ReadSingleFromStream(ms);
          byte elevationOffsetSizeInBytes = StreamUtils.__Global.ReadByteFromStream(ms);

          uint timeOrigin = StreamUtils.__Global.ReadLongWordFromStream(ms); // UTC expressed as Unix time in seconds.
          byte timeOffsetSizeInBytes = StreamUtils.__Global.ReadByteFromStream(ms);

          log.LogDebug($"Subgrid elevation origin in {elevationOrigin}");

          // Protobuf is limited to single dimension arrays so we cannot use the [32,32] layout used by other patch executors.
          const int arrayLength = 32 * 32;
          var cells = new PatchCellHeightResult[arrayLength];

          for (var j = 0; j < arrayLength; j++)
          {
            ushort elevationOffsetDelta;
            uint time = 0;

            switch (elevationOffsetSizeInBytes)
            {
              // Return raw elevation offset delta and the client will determine the elevation using the following equation:
              // float elevation = elevationOffsetDelta != 0xffff ? (float)(elevationOrigin + (elevOffset / 1000.0)) : -100000;

              // Also increment all non-null values by 1. The consumer will subtract 1 from all non zero values to determine the true elevation offset.
              // These efforts are to further help the variant length operations that Protobuf applies to the byte stream.
              case 1:
                {
                  uint elevationOffset = StreamUtils.__Global.ReadByteFromStream(ms);
                  elevationOffsetDelta = (ushort)(elevationOffset != 0xff ? elevationOffset + 0x1 : 0x0);

                  break;
                }
              case 2:
                {
                  uint elevationOffset = StreamUtils.__Global.ReadWordFromStream(ms);
                  elevationOffsetDelta = (ushort)(elevationOffset != 0xffff ? elevationOffset + 0x1 : 0x0);

                  break;
                }
              case 4:
                {
                  uint elevationOffset = StreamUtils.__Global.ReadLongWordFromStream(ms);
                  elevationOffsetDelta = (ushort)(elevationOffset != 0xffffffff ? elevationOffset + 0x1 : 0x0);

                  break;
                }
              default:
                {
                  throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    $"Invalid elevation offset size, '{elevationOffsetSizeInBytes}'."));
                }
            }

            switch (timeOffsetSizeInBytes)
            {
              case 1:
                {
                  var timeOffset = StreamUtils.__Global.ReadByteFromStream(ms);
                  time = (uint)(timeOffset != 0xff ? timeOffset : 0x0);

                  break;
                }
              case 2:
                {
                  var timeOffset = StreamUtils.__Global.ReadWordFromStream(ms);
                  time = (uint)(timeOffset != 0xffff ? timeOffset : 0x0);

                  break;
                }
              case 4:
                {
                  var timeOffset = StreamUtils.__Global.ReadLongWordFromStream(ms);
                  time = timeOffset != 0xffffffff ? timeOffset : 0x0;

                  break;
                }
              default:
                {
                  throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    $"Invalid time offset size, '{timeOffsetSizeInBytes}'."));
                }
            }

            cells[j] = PatchCellHeightResult.Create(elevationOffsetDelta, includeTimeOffsets ? time : uint.MaxValue);
          }

          subgrids[i] = PatchSubgridOriginProtobufResult.Create(subgridOriginX, subgridOriginY, elevationOrigin, includeTimeOffsets ? timeOrigin : uint.MaxValue, cells);
        }

        return PatchResult.Create(cellSize,
          numSubgridsInPatch,
          dataPatchSubgridCount,
          subgrids);
      }
    }
  }
}
