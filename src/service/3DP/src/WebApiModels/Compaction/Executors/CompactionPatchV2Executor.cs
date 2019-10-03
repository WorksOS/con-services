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
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using System.Collections.Generic;

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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      // Note: The numPatches out parameter is ignored in favour of the same value returned in the PatchResult proper. This will be removed
      // in due course once the breaking modifications process is agreed with BC.
      try
      {
        var request = CastRequestObjectTo<PatchRequest>(item);
#if RAPTOR
        if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_PATCHES") ?? false)
        {
#endif
          var patchDataRequest = new PatchDataRequest(
            request.ProjectUid.Value,
            request.Filter1,
            request.Filter2,
            request.Mode,
            request.PatchNumber,
            request.PatchSize,
            AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));

          var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(patchDataRequest, "/patches", customHeaders);

          return fileResult.Length > 0
              ? ConvertPatchResult(fileResult, true)
              : CreateNullPatchReturnedResult();
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

    private ContractExecutionResult CreateNullPatchReturnedResult()
    {
      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null patch returned");
    }
#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(PatchRequest request)
    {
      var filter1 = RaptorConverters.ConvertFilter(request.Filter1, request.ProjectId, raptorClient);
      var filter2 = RaptorConverters.ConvertFilter(request.Filter2, request.ProjectId, raptorClient);
      var volType = RaptorConverters.ConvertVolumesType(request.ComputeVolType);

      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
        RaptorConverters.AdjustFilterToFilter(ref filter1, filter2);
      }

      RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.Mode, request.ComputeVolType);

      var raptorResult = raptorClient.RequestDataPatchPageWithTime(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
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
        return patch != null
          ? ConvertPatchResult(patch, request.IncludeTimeOffsets)
          : CreateNullPatchReturnedResult();
      }

      throw CreateServiceException<CompactionPatchV2Executor>((int)raptorResult);
    }
#endif
    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    private PatchSubgridsProtobufResult ConvertPatchResult(Stream stream, bool includeTimeOffsets)
    {
      using (var reader = new BinaryReader(stream))
      {
        // only set if patchId/patchNumber = 0
        // else -1
        var totalPatchesRequired = reader.ReadInt32();  
        var numSubgridsInPatch = reader.ReadInt32(); // actual count in this patch
        var numSubgridsInResult = numSubgridsInPatch; // actual count returned
        double cellSize = reader.ReadDouble();

        var subgrids = new List<PatchSubgridOriginProtobufResult>();

        for (var i = 0; i < numSubgridsInPatch; i++)
        {
          var subgridOriginX = reader.ReadDouble();
          var subgridOriginY = reader.ReadDouble();
          var isNull = reader.ReadBoolean();

          log.LogDebug($"Subgrid {i + 1} in patch has world origin of {subgridOriginX}:{subgridOriginY}. IsNull?:{isNull}");

          if (isNull)
          {
            --numSubgridsInResult;
            continue;
          }

          float elevationOrigin = reader.ReadSingle();
          byte elevationOffsetSizeInBytes = reader.ReadByte();

          uint timeOrigin = reader.ReadUInt32(); // UTC expressed as Unix time in seconds.
          byte timeOffsetSizeInBytes = reader.ReadByte();

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
                  uint elevationOffset = reader.ReadByte();
                  elevationOffsetDelta = (ushort)(elevationOffset != 0xff ? elevationOffset + 0x1 : 0x0);

                  break;
                }
              case 2:
                {
                  uint elevationOffset = reader.ReadUInt16();
                  elevationOffsetDelta = (ushort)(elevationOffset != 0xffff ? elevationOffset + 0x1 : 0x0);

                  break;
                }
              case 4:
                {
                  uint elevationOffset = reader.ReadUInt32();
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
                  var timeOffset = reader.ReadByte();
                  time = (uint)(timeOffset != 0xff ? timeOffset : 0x0);

                  break;
                }
              case 2:
                {
                  var timeOffset = reader.ReadUInt16();
                  time = (uint)(timeOffset != 0xffff ? timeOffset : 0x0);

                  break;
                }
              case 4:
                {
                  var timeOffset = reader.ReadUInt32();
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

          subgrids.Add(PatchSubgridOriginProtobufResult.Create(subgridOriginX, subgridOriginY, elevationOrigin, includeTimeOffsets ? timeOrigin : uint.MaxValue, cells));
        }

        log.LogDebug($"{nameof(ConvertPatchResult)} totalPatchesRequired: {totalPatchesRequired} numSubgridsInPatch: {numSubgridsInPatch} numSubgridsInResult: {numSubgridsInResult} subgridsCount: {subgrids.Count}");
        return PatchSubgridsProtobufResult.Create(cellSize,
          numSubgridsInResult, // actual number is less those where all null (if (isNull) above)
          totalPatchesRequired,
          subgrids.ToArray());
      }
    }

  }
}
