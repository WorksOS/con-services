using System;
using System.Collections.Generic;
using System.Net;
using ASNode.CMVChange.RPC;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
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
      CMVChangeSummaryRequest request = item as CMVChangeSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(CMVChangeSummaryRequest));

      try
      {
        TASNodeCMVChangeResult result = new TASNodeCMVChangeResult();

        if (!bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_CMV"), out var useTrexGateway))
          useTrexGateway = false;
        
        if (useTrexGateway)
        {
          var cmvChangeDetailsRequest = new CMVChangeDetailsRequest(request.ProjectUid, request.Filter, request.CMVChangeSummaryValues);
          
          return trexCompactionDataProxy.SendCMVChangeDetailsRequest(cmvChangeDetailsRequest, customHeaders).Result;
        }
        else
        {
          TASNodeCMVChangeSettings settings = new TASNodeCMVChangeSettings(request.CMVChangeSummaryValues);

          var raptorResult = raptorClient.GetCMVChangeSummary(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0,
              TASNodeCancellationDescriptorType.cdtCMVChange),
            settings,
            RaptorConverters.ConvertFilter(request.FilterId, request.Filter, request.ProjectId, null, null,
              new List<long>()),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
            out result);
          if (raptorResult == TASNodeErrorStatus.asneOK)
          {
            return ConvertResult(result);
          }

          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested CMV change summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
        }
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
