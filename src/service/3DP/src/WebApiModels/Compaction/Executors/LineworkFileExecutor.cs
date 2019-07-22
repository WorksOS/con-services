using System;
using System.Threading.Tasks;
using ASNode.DXF.RequestBoundaries.RPC;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Executor for processing DXF linework files.
  /// </summary>
  public class LineworkFileExecutor : RequestExecutorContainer
  {
    public LineworkFileExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<LineworkRequest>(item);

      return UseTRexGateway("ENABLE_TREX_GATEWAY_LINEWORKFILE")
        ? ProcessForTRex(request)
        : ProcessForRaptor(request);
    }

    private DxfLineworkFileResult ProcessForTRex(LineworkRequest request)
    {
      throw new NotImplementedException("TRex Gateway not yet implemented for LineworkFileExecutor");
    }

    private DxfLineworkFileResult ProcessForRaptor(LineworkRequest request)
    {
      var returnResult = TASNodeErrorStatus.asneUnknown;

      try
      {
        var customDescriptor = new TVLPDDesignDescriptor();
        customDescriptor.Init(0, string.Empty, string.Empty, request.DxfFileDescriptor.Path, request.DxfFileDescriptor.FileName, 0);

        log.LogDebug($"{nameof(LineworkFileExecutor)}::{nameof(ProcessForRaptor)}() : {nameof(TVLPDDesignDescriptor)} = {JsonConvert.SerializeObject(customDescriptor)}");

        var args = new TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args
        {
          DataModelID = request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          LineworkDescriptor = customDescriptor,
          MaxVerticesPerBoundary = request.NumberOfVerticesPerBoundary,
          MaxBoundariesToProcess = request.NumberOfBoundariesToProcess,
          CoordSystemFileName = request.CoordinateSystemFileDescriptor.FileName,
          LineworkUnits = (TVLPDDistanceUnits)request.LineworkUnits
        };

        returnResult = raptorClient.GetBoundariesFromLinework(args, out var lineworksBoundary);

        log.LogInformation($"RequestBoundariesFromLinework: result: {JsonConvert.SerializeObject(returnResult)}");

        if (returnResult != TASNodeErrorStatus.asneOK)
        {
          throw CreateServiceException<LineworkFileExecutor>((int)returnResult);
        }

        return new DxfLineworkFileResult(returnResult, "", lineworksBoundary);
      }
      catch (ServiceException ex)
      {
        var errorMessage = ex.GetResult.Message;

        log.LogError($"RequestBoundariesFromLinework: exception {errorMessage}");

        return new DxfLineworkFileResult(returnResult, errorMessage, null);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
