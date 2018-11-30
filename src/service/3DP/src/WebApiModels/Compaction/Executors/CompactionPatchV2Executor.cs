﻿using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SVOICVolumeCalculationsDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
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
      // Note: The numPatches out parameter is ignored in favour of the same value returned in the PatchResult proper. This will be removed
      // in due course once the breaking modifications process is agreed with BC.
      try
      {
        var request = item as PatchRequest;

        if (request == null)
          ThrowRequestTypeCastException<PatchRequest>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_PATCHES"), out var useTrexGateway);

        if (useTrexGateway)
        {
          var patchDataRequest = new PatchDataRequest(
            request.ProjectUid, 
            request.Filter1, 
            request.Filter2, 
            request.Mode, 
            request.PatchNumber, 
            request.PatchSize); 

          var fileResult = trexCompactionDataProxy.SendProductionDataPatchRequest(patchDataRequest, customHeaders).Result;

          return fileResult.Length > 0
              ? ConvertPatchResult(fileResult, true)
              : CreateNullPatchReturnedResult();
        }

        return ProcessWithRaptor(request);
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

    private ContractExecutionResult ProcessWithRaptor(PatchRequest request)
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
        return patch != null
          ? ConvertPatchResult(patch, request.IncludeTimeOffsets)
          : CreateNullPatchReturnedResult();
      }

      throw CreateServiceException<CompactionPatchV2Executor>((int)raptorResult);
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    private PatchResult ConvertPatchResult(Stream stream, bool includeTimeOffsets)
    {
      var numSubgridsInPatch = StreamUtils.__Global.ReadIntegerFromStream(stream);
      var dataPatchSubgridCount = StreamUtils.__Global.ReadIntegerFromStream(stream);
      double cellSize = StreamUtils.__Global.ReadDoubleFromStream(stream);

      var subgrids = new PatchSubgridResultBase[dataPatchSubgridCount];

      for (var i = 0; i < dataPatchSubgridCount; i++)
      {
        var subgridOriginX = StreamUtils.__Global.ReadDoubleFromStream(stream);
        var subgridOriginY = StreamUtils.__Global.ReadDoubleFromStream(stream);
        var isNull = StreamUtils.__Global.ReadBooleanFromStream(stream);

        log.LogDebug($"Subgrid {i + 1} in patch has world origin of {subgridOriginX}:{subgridOriginY}. IsNull?:{isNull}");

        if (isNull)
        {
          continue;
        }

        float elevationOrigin = StreamUtils.__Global.ReadSingleFromStream(stream);
        byte elevationOffsetSizeInBytes = StreamUtils.__Global.ReadByteFromStream(stream);

        uint timeOrigin = StreamUtils.__Global.ReadLongWordFromStream(stream); // UTC expressed as Unix time in seconds.
        byte timeOffsetSizeInBytes = StreamUtils.__Global.ReadByteFromStream(stream);

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
                uint elevationOffset = StreamUtils.__Global.ReadByteFromStream(stream);
                elevationOffsetDelta = (ushort)(elevationOffset != 0xff ? elevationOffset + 0x1 : 0x0);

                break;
              }
            case 2:
              {
                uint elevationOffset = StreamUtils.__Global.ReadWordFromStream(stream);
                elevationOffsetDelta = (ushort)(elevationOffset != 0xffff ? elevationOffset + 0x1 : 0x0);

                break;
              }
            case 4:
              {
                uint elevationOffset = StreamUtils.__Global.ReadLongWordFromStream(stream);
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
                var timeOffset = StreamUtils.__Global.ReadByteFromStream(stream);
                time = (uint)(timeOffset != 0xff ? timeOffset : 0x0);

                break;
              }
            case 2:
              {
                var timeOffset = StreamUtils.__Global.ReadWordFromStream(stream);
                time = (uint)(timeOffset != 0xffff ? timeOffset : 0x0);

                break;
              }
            case 4:
              {
                var timeOffset = StreamUtils.__Global.ReadLongWordFromStream(stream);
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
