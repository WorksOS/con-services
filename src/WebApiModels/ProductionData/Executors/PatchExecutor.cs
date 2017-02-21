using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICVolumeCalculationsDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
  public class PatchExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock RaptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public PatchExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PatchExecutor()
    {
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      PatchRequest request = item as PatchRequest;

      MemoryStream patch;
      int numPatches;

      // Note: The numPatches out parameter is ignored in favour of the same value returned in the PatchResult proper. This will be removed
      // in due course once the breaking modifications process is agreed with BC.
      try
      {
        TICFilterSettings filter1 =
          RaptorConverters.ConvertFilter(request.filterId1, request.filter1, request.projectId);
        TICFilterSettings filter2 =
          RaptorConverters.ConvertFilter(request.filterId2, request.filter2, request.projectId);
        TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.computeVolType);

        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
          RaptorConverters.AdjustFilterToFilter(filter1, filter2);

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref filter1, ref filter2, request.mode, request.computeVolType);

        TASNodeErrorStatus raptorResult = raptorClient.RequestDataPatchPage(request.projectId ?? -1,
                ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                        TASNodeCancellationDescriptorType.cdtDataPatches),
                RaptorConverters.convertDisplayMode(request.mode),
                RaptorConverters.convertColorPalettes(request.palettes, request.mode),
                request.renderColorValues,
                filter1,
                filter2,
                RaptorConverters.convertOptions(null, request.liftBuildSettings,
                        request.computeVolNoChangeTolerance, request.filterLayerMethod, request.mode),
                RaptorConverters.DesignDescriptor(request.designDescriptor),
                volType,
                request.patchNumber,
                request.patchSize,
                out patch,
                out numPatches);

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          if (patch != null)
          {
            PatchResult rawResult = PatchResult.CreatePatchResult(patch.ToArray());
            result = convertPatchResult(rawResult);
          }
          else
          {
            result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null patch returned");
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                                     string.Format("Failed to get requested patch with error: {0}.", ContractExecutionStates.FirstNameWithOffset((int)raptorResult))));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }









    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }




    private PatchResultStructured convertPatchResult(PatchResult patch)
    {
      MemoryStream ms = new MemoryStream(patch.PatchData);

      int totalNumPatchesRequired = StreamUtils.__Global.ReadIntegerFromStream(ms);
      bool valuesRenderedToColors = StreamUtils.__Global.ReadBooleanFromStream(ms);
      int numSubgridsInPatch = StreamUtils.__Global.ReadIntegerFromStream(ms);
      double cellSize = StreamUtils.__Global.ReadDateTimeFromStream(ms);

      //log.IfDebugFormat("[{4}] Patch {0},{5} bytes of {1} contains {2} subgrids, cellsize={3}", patchNum, numPatches, numSubgridsInPatch, cellSize, DateTime.Now, patch.Length);

      List<PatchSubgridResult> subgrids = new List<PatchSubgridResult>();
      for (int i = 0; i < numSubgridsInPatch; i++)
      {
        int cellOriginX = StreamUtils.__Global.ReadIntegerFromStream(ms);
        int cellOriginY = StreamUtils.__Global.ReadIntegerFromStream(ms);
        bool isNull = StreamUtils.__Global.ReadBooleanFromStream(ms);

        log.LogDebug("Subgrid {0} in patch has cell origin of {1}:{2}. IsNull?:{3}", i + 1, cellOriginX, cellOriginY, isNull);

        float elevationOrigin = 0;
        PatchCellResult[,] cells = null;
        if (!isNull)
        {
          elevationOrigin = StreamUtils.__Global.ReadSingleFromStream(ms);

          log.LogDebug("Subgrid elevation origin in {0}", elevationOrigin);

          cells = new PatchCellResult[32, 32];//Raymond's code had [SubGridTreesDecls.__Global.kSubGridTreeDimension, SubGridTreesDecls.__Global.kSubGridTreeDimension];
          for (int j = 0; j < 32; j++)
          {
            for (int k = 0; k < 32; k++)
            {
              float _elevation = -100000;

              ushort elevOffset = StreamUtils.__Global.ReadWordFromStream(ms);
              if (elevOffset != 0xffff)
                _elevation = (float)(elevationOrigin + (elevOffset / 1000.0));

              uint _colour = valuesRenderedToColors ? StreamUtils.__Global.ReadLongWordFromStream(ms) : 0;

              log.LogDebug("Cell {0}:{1}, elevation: {2}, colour: {3}", j, k, _elevation, _colour);

              cells[j, k] = PatchCellResult.CreatePatchCellResult(_elevation, 0, valuesRenderedToColors ? _colour : 0);
            }
          }
        }
        subgrids.Add(PatchSubgridResult.CreatePatchSubgridResult(cellOriginX, cellOriginY, isNull, elevationOrigin, cells));
      }

      return PatchResultStructured.CreatePatchResultStructured(cellSize, numSubgridsInPatch, totalNumPatchesRequired,
          valuesRenderedToColors, subgrids.ToArray());
    }
  }


}