using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using SVOICVolumeCalculationsDecls;
#endif
using Microsoft.AspNetCore.Mvc;
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

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionPatchV2ExecutorSimple : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionPatchV2ExecutorSimple()
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

    private PatchResultSimple ConvertPatchResult(Stream stream, bool includeTimeOffsets)
    {
      using (var reader = new BinaryReader(stream))
      {
        // only set if patchId/patchNumber = 0
        // else -1
        var totalPatchesRequired = reader.ReadInt32();  
        var numSubgridsInPatch = reader.ReadInt32(); // actual count in this patch
        double cellSize = reader.ReadDouble();

        var cells = new List<PatchCellSimpleResult>(); 

        for (var i = 0; i < numSubgridsInPatch; i++)
        {
          var subgridOriginX = reader.ReadDouble();
          var subgridOriginY = reader.ReadDouble();
          var isNull = reader.ReadBoolean();

          log.LogDebug($"Subgrid {i + 1} in patch has world origin of {subgridOriginX}:{subgridOriginY}. IsNull?:{isNull}");

          if (isNull)
          {
            continue;
          }

          float elevationOrigin = reader.ReadSingle();
          byte elevationOffsetSizeInBytes = reader.ReadByte();

          uint timeOrigin = reader.ReadUInt32(); // UTC expressed as Unix time in seconds.
          byte timeOffsetSizeInBytes = reader.ReadByte();

          log.LogDebug($"Subgrid elevation origin in {elevationOrigin}");

          const int arrayLength = 32 * 32;
          for (var j = 0; j < arrayLength; j++)
          {
            float elevation = float.NaN;
            uint eventTime = 0;

            switch (elevationOffsetSizeInBytes)
            {
              case 1:
              {
                uint elevationOffset = reader.ReadByte();
                elevation = (float) (elevationOffset != 0xff ? (elevationOrigin + (elevationOffset / 1000.0)) : float.NaN);

                break;
              }
              case 2:
              {
                uint elevationOffset = reader.ReadUInt16();
                elevation = (float) (elevationOffset != 0xffff ? (elevationOrigin + (elevationOffset / 1000.0)) : float.NaN);

                break;
              }
              case 4:
              {
                uint elevationOffset = reader.ReadUInt32();
                elevation = (float) (elevationOffset != 0xffffffff ? (elevationOrigin + (elevationOffset / 1000.0)) : float.NaN);

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
                eventTime = (uint) (timeOffset != 0xff ? timeOffset + timeOrigin : 0x0);

                break;
              }
              case 2:
              {
                var timeOffset = reader.ReadUInt16();
                eventTime = (uint) (timeOffset != 0xffff ? timeOffset + timeOrigin : 0x0);

                break;
              }
              case 4:
              {
                var timeOffset = reader.ReadUInt32();
                eventTime = timeOffset != 0xffffffff ? timeOffset + timeOrigin : 0x0;

                break;
              }
              default:
              {
                throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  $"Invalid time offset size, '{timeOffsetSizeInBytes}'."));
              }
            }

            if (!float.IsNaN(elevation) )
            //&& cells.Count == 0) // todoJeannie
            {
              var cellY = Math.DivRem(j, 32, out int cellX); /* todo*/
              cells.Add(PatchCellSimpleResult.Create(Math.Round(((subgridOriginX + (cellSize / 2)) + (cellSize * cellX)), 5),
                Math.Round(((subgridOriginY + (cellSize / 2)) + (cellSize * cellY)), 5),
                Math.Round(elevation, 3), eventTime));
            }
          }
        }

        return PatchResultSimple.Create(cellSize,
          numSubgridsInPatch,
          totalPatchesRequired,
          cells.ToArray());
      }
    }

  }
}
