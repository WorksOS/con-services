using System;
using System.Threading.Tasks;
using ASNode.DXF.RequestBoundaries.RPC;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// 
  /// </summary>
  public class LineworkFileExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<LineworkRequest>(item);

        return UseTRexGateway("ENABLE_TREX_GATEWAY_LINEWORKFILE")
          ? ProcessForTRex(request)
          : ProcessForRaptor(request);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
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
        customDescriptor.Init(0, string.Empty, string.Empty, request.FileDescriptor.Path, request.FileDescriptor.FileName, 0);

        log.LogDebug($"{nameof(LineworkFileExecutor)}: {nameof(TVLPDDesignDescriptor)} = {JsonConvert.SerializeObject(customDescriptor)}");

        var args = new TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args
        {
          DataModelID = request.ProjectId ?? -1,
          LineworkDescriptor = customDescriptor,
          MaxVerticesPerBoundary = request.NumberOfVerticesPerBoundary,
          MaxBoundariesToProcess = request.NumberOfBoundariesToProcess,
          CoordSystemFileName = request.CoordSystemFileName,
          LineworkUnits = request.LineworkUnits
        };

        // TODO (Aaron) Handled non success response codes, e.g. 74 = asneNoBoundariesInLineworkFile

        returnResult = raptorClient.GetBoundariesFromLinework(args, out var lineworksBoundary);

        log.LogInformation($"RequestBoundariesFromLinework: result: {JsonConvert.SerializeObject(returnResult)}");

        return new DxfLineworkFileResult(returnResult, "", lineworksBoundary);
      }
      catch (Exception ex)
      {
        log.LogError($"RequestBoundariesFromLinework: exception {ex.Message}");
        return new DxfLineworkFileResult(returnResult, "", null);
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
