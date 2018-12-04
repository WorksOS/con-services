using System;
using System.Threading.Tasks;
using ASNode.DXF.RequestBoundaries.RPC;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// 
  /// </summary>
  public class LineworkFileExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<LineworkRequest>(item);

      if (UseTRexGateway("ENABLE_TREX_GATEWAY"))
      {
        return CallTRexEndpoint(request);
      }

      if (UseRaptorGateway("ENABLE_RAPTOR_GATEWAY"))
      {
        var result = CallRaptorEndpoint(request);
      }

      return ContractExecutionResult.ErrorResult();
    }

    private ContractExecutionResult CallTRexEndpoint(LineworkRequest request)
    {
      throw new NotImplementedException();
    }

    private ContractExecutionResult CallRaptorEndpoint(LineworkRequest request)
    {
      var returnResult = TASNodeErrorStatus.asneUnknown;

      try
      {
        var args = new TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args
        {
          DataModelID = request.ProjectId ?? -1,
          LineworkDescriptor = RaptorConverters.DesignDescriptor(request.LineworkDescriptor),
          MaxVerticesPerBoundary = request.NumberOfVerticesPerBoundary,
          MaxBoundariesToProcess = request.NumberOfBoundariesToProcess,
          CoordSystemFileName = request.CoordSystemFileName,
          LineworkUnits = request.LineworkUnits
        };

        returnResult = raptorClient.GetBoundariesFromLinework(args, out var lineworksBoundary);

        log.LogInformation($"RequestBoundariesFromLinework: result: {JsonConvert.SerializeObject(returnResult)}");
      }
      catch (Exception ex)
      {
        log.LogError($"RequestBoundariesFromLinework: exception {ex.Message}");
        return new ContractExecutionResult((int)returnResult);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      throw new NotImplementedException();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
