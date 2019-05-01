using System;
using System.Collections.Generic;
#if RAPTOR
using ASNode.CMVChange.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
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

#if RAPTOR
    private CMVChangeSummaryResult ConvertResult(TASNodeCMVChangeResult result)
    {
      return new CMVChangeSummaryResult
      (
        result.Values,
        result.CoverageArea
      );
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CMVChangeSummaryRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_CMV"))
        {
#endif
          var cmvChangeDetailsRequest = new CMVChangeDetailsRequest(request.ProjectUid, request.Filter, request.CMVChangeSummaryValues);
          return trexCompactionDataProxy.SendDataPostRequest<CMVChangeSummaryResult, CMVChangeDetailsRequest>(cmvChangeDetailsRequest, "/cmv/percentchange", customHeaders).Result;
#if RAPTOR
        }
        new TASNodeCMVChangeResult();

        TASNodeCMVChangeSettings settings = new TASNodeCMVChangeSettings(request.CMVChangeSummaryValues);

        var raptorResult = raptorClient.GetCMVChangeSummary(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtCMVChange),
          settings,
          RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient, overrideAssetIds: new List<long>()),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out var result);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(result);

        throw CreateServiceException<CMVChangeSummaryExecutor>((int)raptorResult);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }
  }
}
