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

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class PatchExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PatchExecutor()
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
                  request.ComputeVolNoChangeTolerance, request.FilterLayerMethod, request.Mode, request.SetSummaryDataLayersVisibility),
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

    private PatchResultRenderedColors ConvertPatchResult(byte[] patch)
    {
      var ms = new MemoryStream(patch);

      var totalNumPatchesRequired = StreamUtils.__Global.ReadIntegerFromStream(ms);
      var valuesRenderedToColors = StreamUtils.__Global.ReadBooleanFromStream(ms);
      var numSubgridsInPatch = StreamUtils.__Global.ReadIntegerFromStream(ms);
      double cellSize = StreamUtils.__Global.ReadDateTimeFromStream(ms);
      var subgrids = new PatchSubgridResultBase[numSubgridsInPatch];

      for (var i = 0; i < numSubgridsInPatch; i++)
      {
        var cellOriginX = StreamUtils.__Global.ReadIntegerFromStream(ms);
        var cellOriginY = StreamUtils.__Global.ReadIntegerFromStream(ms);
        var isNull = StreamUtils.__Global.ReadBooleanFromStream(ms);

        log.LogDebug($"Subgrid {i + 1} in patch has cell origin of {cellOriginX}:{cellOriginY}. IsNull?:{isNull}");

        float elevationOrigin = 0;
        PatchCellResult[,] cells = null;

        if (!isNull)
        {
          elevationOrigin = StreamUtils.__Global.ReadSingleFromStream(ms);

          log.LogDebug($"Subgrid elevation origin in {elevationOrigin}");

          // Raptor uses: [SubGridTreesDecls.__Global.kSubGridTreeDimension, SubGridTreesDecls.__Global.kSubGridTreeDimension];
          cells = new PatchCellResult[32, 32];

          for (var j = 0; j < 32; j++)
          {
            for (var k = 0; k < 32; k++)
            {
              var elevOffset = StreamUtils.__Global.ReadWordFromStream(ms);
              var elevation = elevOffset != 0xffff ? (float)(elevationOrigin + (elevOffset / 1000.0)) : -100000;
              var colour = valuesRenderedToColors ? StreamUtils.__Global.ReadLongWordFromStream(ms) : 0;

              cells[j, k] = PatchCellResult.Create(elevation, 0, valuesRenderedToColors ? colour : 0);
            }
          }
        }

        subgrids[i] = PatchSubgridResult.Create(cellOriginX, cellOriginY, isNull, elevationOrigin, cells);
      }

      return PatchResultRenderedColors.Create(
        cellSize,
        numSubgridsInPatch,
        totalNumPatchesRequired,
        valuesRenderedToColors,
        subgrids);
    }
  }
}
