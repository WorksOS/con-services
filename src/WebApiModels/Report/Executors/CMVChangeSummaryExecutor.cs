using System;
using System.Collections.Generic;
using System.Net;
using ASNode.CMVChange.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
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

      try
      {
        TASNodeCMVChangeResult result = new TASNodeCMVChangeResult();

        TASNodeCMVChangeSettings settings = new TASNodeCMVChangeSettings(request.CMVChangeSummaryValues);

        var raptorResult = raptorClient.GetCMVChangeSummary(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtCMVChange),
          settings,
          RaptorConverters.ConvertFilter(request.filterId, request.filter, request.ProjectId, null, null,
            new List<long>()),
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out result);
        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          return ConvertResult(result);
        }

        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
          $"Failed to get requested CMV change summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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
