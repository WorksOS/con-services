using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using System.Collections.Generic;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Common.Filters.Utilities;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionSinglePatchExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionSinglePatchExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      // Note: The numPatches out parameter is ignored in favor of the same value returned in the PatchResult proper. This will be removed
      // in due course once the breaking modifications process is agreed with BC.
      try
      {
        var request = CastRequestObjectTo<PatchRequest>(item);

        if (request.ComputeVolType == VolumesType.Between2Filters)
          FilterUtilities.AdjustFilterToFilter(request.Filter1, request.Filter2);
        }

        var filter1 = request.Filter1;
        var filter2 = request.Filter2;

        FilterUtilities.ReconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.Mode, request.ComputeVolType);

        var patchDataRequest = new PatchDataRequest(
          request.ProjectUid.Value,
          filter1,
          filter2,
          request.Mode,
          request.PatchNumber,
          request.PatchSize,
          AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
          AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));

        var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(patchDataRequest, "/patches", customHeaders);

        return fileResult.Length > 0
            ? ConvertPatchResult(fileResult, true)
            : CreateNullPatchReturnedResult();
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

    protected sealed override void ProcessErrorCodes()
    { }

    private PatchSubgridsProtobufResult ConvertPatchResult(Stream stream, bool includeTimeOffsets)
    {
      using (var reader = new BinaryReader(stream))
      {
        // only set if patchId/patchNumber = 0 ( else -1 )
        var totalPatchesRequired = reader.ReadInt32();
        var subGridsInPatch = reader.ReadInt32();    // actual count in this patch
        var subGridsWithDataToReturn = subGridsInPatch; // sub-grids with data to be returned
        var cellSize = reader.ReadDouble();

        var subgrids = new List<PatchSubgridOriginProtobufResult>();

        for (var i = 0; i < subGridsInPatch; i++)
        {
          var subgridOriginX = reader.ReadDouble();
          var subgridOriginY = reader.ReadDouble();
          var isNull = reader.ReadBoolean();

          if (isNull)
          {
            --subGridsWithDataToReturn;
            continue;
          }

          var elevationOrigin = reader.ReadSingle();
          var elevationOffsetSizeInBytes = reader.ReadByte();

          var timeOrigin = reader.ReadUInt32(); // UTC expressed as Unix time in seconds.
          var timeOffsetSizeInBytes = reader.ReadByte();

          // Protobuf is limited to single dimension arrays so we cannot use the [32,32] layout used by other patch executors.
          const int arrayLength = 32 * 32;
          var elevationOffsets = new ushort[arrayLength];
          var timeOffsets = new uint[arrayLength];

          for (var j = 0; j < arrayLength; j++)
          {
            ushort elevationOffsetDelta;
            uint time = 0;

            switch (elevationOffsetSizeInBytes)
            {
              // Return raw elevation offset delta and the client will determine the elevation using the following equation:
              // float elevation = elevationOffsetDelta != 0xffff ? (float)(elevationOrigin + (elevOffset / 1000.0)) : -100000;

              // Cells[] is in column major order i.e. 0,0, 0,1; 0,2 .. 0.31 1,0, 1,1, 1.2..1.31

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

            // timeOrigin and time Unix seconds
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

            elevationOffsets[j] = elevationOffsetDelta;
            timeOffsets[j] = includeTimeOffsets ? time : uint.MaxValue;
          }

          subgrids.Add(PatchSubgridOriginProtobufResult.Create(Math.Round(subgridOriginX, 5), Math.Round(subgridOriginY, 5), elevationOrigin, includeTimeOffsets ? timeOrigin : uint.MaxValue, elevationOffsets, timeOffsets));
          // test: var doubleArrayResult = (new CompactionSinglePatchPackedResult()).UnpackSubgrid(cellSize, subgrids[subgrids.Count - 1]);

        }

        log.LogDebug($"{nameof(ConvertPatchResult)} totalPatchesRequired: {totalPatchesRequired} subGridsInPatch: {subGridsInPatch} subGridsWithDataToReturn: {subGridsWithDataToReturn} subgridsCount: {subgrids.Count}");
        return PatchSubgridsProtobufResult.Create(cellSize, subgrids.ToArray());
      }
    }
  }
}
