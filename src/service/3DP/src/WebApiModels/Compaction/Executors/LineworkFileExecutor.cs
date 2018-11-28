using System;
using System.Threading.Tasks;
using ASNode.DXF.RequestBoundaries.RPC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

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
        return await CallTRexEndpoint(request).ConfigureAwait(false);
      }

      if (UseRaptorGateway("ENABLE_RAPTOR_GATEWAY"))
      {
        var lineworkRequest = LineworkRequest.Create(0, null, TVLPDDistanceUnits.vduImperialFeet, 0, 0, "");

        var result = CallRaptorEndpoint(lineworkRequest);
        if (result.Code == 0)
        {

        }
        else
        {
        }
      }

      return ContractExecutionResult.ErrorResult();
    }

    private Task<ContractExecutionResult> CallTRexEndpoint(LineworkRequest request)
    {
      throw new NotImplementedException();
    }

    private TagFileDirectSubmissionResult CallRaptorEndpoint(LineworkRequest request)
    {
      try
      {
        var returnResult = raptorClient.GetBoundariesFromLinework(
          new TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args
          {
            DataModelID = request.ProjectId ?? -1,
            LineworkDescriptor = RaptorConverters.DesignDescriptor(request.LineworkDescriptor)
          },
        out var lineworksBoundary);

        log.LogInformation($"PostTagFile (Direct Raptor): result: {JsonConvert.SerializeObject(returnResult)}");
      }
      catch (Exception ex)
      {
        log.LogError($"PostTagFile (Direct Raptor): exception {ex.Message}");
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprUnknown));
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
