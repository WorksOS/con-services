using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Utilities;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class PatchExecutor : TbcExecutorHelper
  {
    private const float CONST_MIN_ELEVATION = -100000;

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PatchExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<PatchRequest>(item);

        var filter1 = request.Filter1;
        var filter2 = request.Filter2;
        if (request.ComputeVolType == VolumesType.Between2Filters)
        {
          (filter1, filter2) = FilterUtilities.AdjustFilterToFilter(request.Filter1, request.Filter2);
        }
        else
        {
          (filter1, filter2) = FilterUtilities.ReconcileTopFilterAndVolumeComputationMode(filter1, filter2, request.Mode, request.ComputeVolType);
        }

        await PairUpAssetIdentifiers(request.ProjectUid.Value, filter1, filter2);
        await PairUpImportedFileIdentifiers(request.ProjectUid.Value, request.DesignDescriptor, filter1, filter2);
        
        var patchDataRequest = new PatchDataRequest(
          request.ProjectUid.Value,
          filter1,
          filter2,
          request.Mode,
          request.PatchNumber,
          request.PatchSize,
          AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
          AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));
        log.LogDebug($"{nameof(PatchExecutor)} patchDataRequest {JsonConvert.SerializeObject(patchDataRequest)}");

        var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(patchDataRequest, "/patches", customHeaders);

        return fileResult.Length > 0
          ? ConvertPatchResult(fileResult, request)
          : new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null patch returned");
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
    {
    }

    private PatchResultRenderedColors ConvertPatchResult(Stream stream, PatchRequest request)
    {
      using var reader = new BinaryReader(stream);
      var totalPatchesRequired = reader.ReadInt32();
      var subGridsInPatch = reader.ReadInt32();
      var cellSize = reader.ReadDouble();
      var subGrids = new PatchSubgridResultBase[subGridsInPatch];

      for (var i = 0; i < subGridsInPatch; i++)
      {
        var subGridOriginX = reader.ReadDouble();
        var subGridOriginY = reader.ReadDouble();
        var isNull = reader.ReadBoolean();
        log.LogDebug($"SubGrid {i + 1} in patch has cell origin of {subGridOriginX}:{subGridOriginY}. IsNull?:{isNull}");

        float elevationOrigin = 0;
        PatchCellResult[,] cells = null;

        if (!isNull)
        {
          elevationOrigin = reader.ReadSingle();
          var elevationOffsetSizeInBytes = reader.ReadByte();

          // we're going to ignore time for TBC
          var unusedTimeOrigin = reader.ReadUInt32();
          var timeOffsetSizeInBytes = reader.ReadByte();

          cells = new PatchCellResult[32, 32];
          for (var j = 0; j < 32; j++)
          {
            for (var k = 0; k < 32; k++)
            {
              float elevationOffsetDelta = 0;
              switch (elevationOffsetSizeInBytes)
              {
                case 1:
                {
                  var elevationOffset = reader.ReadByte();
                  elevationOffsetDelta = (float) (elevationOffset != 0xff ? (elevationOrigin + (elevationOffset / 1000.0)) : CONST_MIN_ELEVATION);
                  break;
                }
                case 2:
                {
                  var elevationOffset = reader.ReadUInt16();
                  elevationOffsetDelta = (float) (elevationOffset != 0xffff ? (elevationOrigin + (elevationOffset / 1000.0)) : CONST_MIN_ELEVATION);
                  break;
                }
                case 4:
                {
                  var elevationOffset = reader.ReadUInt32();
                  elevationOffsetDelta = (float) (elevationOffset != 0xffffffff ? (elevationOrigin + (elevationOffset / 1000.0)) : CONST_MIN_ELEVATION);
                  break;
                }
                default:
                {
                  throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    $"Invalid elevation offset size, '{elevationOffsetSizeInBytes}'."));
                }
              }

              // timeOrigin and time Unix seconds we are going to ignore this
              switch (timeOffsetSizeInBytes)
              {
                case 1:
                {
                  reader.ReadByte();
                  break;
                }
                case 2:
                {
                  reader.ReadUInt16();
                  break;
                }
                case 4:
                {
                  reader.ReadUInt32();
                  break;
                }
                default:
                {
                  throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    $"Invalid time offset size, '{timeOffsetSizeInBytes}'."));
                }
              }
              cells[j, k] = PatchCellResult.Create(elevationOffsetDelta, 0, ConvertColor(request, elevationOffsetDelta));
            }
          }
        }

        subGrids[i] = PatchSubgridResult.Create((int) subGridOriginX, (int) subGridOriginY, isNull, elevationOrigin, cells);
      }

      log.LogDebug($"{nameof(ConvertPatchResult)} totalPatchesRequired: {totalPatchesRequired} subGridsInPatch: {subGridsInPatch} subgridsCount: {subGrids.Length}");
      return PatchResultRenderedColors.Create(
        cellSize,
        subGridsInPatch,
        totalPatchesRequired,
        request.RenderColorValues,
        subGrids);
    }

    private uint ConvertColor(PatchRequest request, float elevationOffsetDelta)
    {
      if (request.RenderColorValues && Math.Abs(elevationOffsetDelta - CONST_MIN_ELEVATION) > 0.001)
      {
        for (var i = request.Palettes.Count - 1; i >= 0; i--)
        {
          if (elevationOffsetDelta >= request.Palettes[i].Value)
          {
            return request.Palettes[i].Color;
          }
        }
      }

      return (uint) Color.Empty.ToArgb();
    }
  }
}
