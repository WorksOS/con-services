using ASNode.SpeedSummary.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using System;
using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class SummarySpeedExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummarySpeedExecutor()
    {
      ProcessErrorCodes();
    }

    private SpeedSummaryResult ConvertResult(TASNodeSpeedSummaryResult result)
    {

      return new SpeedSummaryResult
      (
        Math.Round(result.AboveTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.BelowTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.MatchTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
        Math.Round(result.CovegareArea, 5)
      );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        SummarySpeedRequest request = item as SummarySpeedRequest;
        new TASNodeSpeedSummaryResult();

        var raptorResult = this.raptorClient.GetSummarySpeed(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          RaptorConverters.ConvertFilter(request.FilterId, request.Filter, request.ProjectId, null, null,
            new List<long>()),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
          out var result);

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          return ConvertResult(result);
        }

        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
          $"Failed to get requested speed summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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
