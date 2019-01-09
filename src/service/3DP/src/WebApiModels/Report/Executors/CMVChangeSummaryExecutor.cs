using System;
using System.Collections.Generic;
using ASNode.CMVChange.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Builds CMV change report from Raptor
  /// </summary>
  public class CMVChangeSummaryExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CMVChangeSummaryExecutor()
    {
      ProcessErrorCodes();
    }

    private CMVChangeSummaryResult ConvertResult(TASNodeCMVChangeResult result)
    {
      return new CMVChangeSummaryResult
      (
        result.Values,
        result.CoverageArea
      );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CMVChangeSummaryRequest>(item);
        
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_CMV"))
        {
          var cmvChangeDetailsRequest = new CMVChangeDetailsRequest(request.ProjectUid, request.Filter, request.CMVChangeSummaryValues);
          return trexCompactionDataProxy.SendCMVChangeDetailsRequest(cmvChangeDetailsRequest, customHeaders).Result;
        }

        new TASNodeCMVChangeResult();

        TASNodeCMVChangeSettings settings = new TASNodeCMVChangeSettings(request.CMVChangeSummaryValues);

        var raptorResult = raptorClient.GetCMVChangeSummary(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtCMVChange),
          settings,
          RaptorConverters.ConvertFilter(request.Filter, overrideAssetIds: new List<long>()),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out var result);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(result);

        throw CreateServiceException<CMVChangeSummaryExecutor>((int)raptorResult);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
