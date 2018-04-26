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
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class SummarySpeedExecutor : RequestExecutorContainer
  {
    private SpeedSummaryResult ConvertResult(TASNodeSpeedSummaryResult result)
    {

      return SpeedSummaryResult.Create
          (
              Math.Round(result.AboveTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.BelowTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.MatchTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.CovegareArea, 5)
          );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      SummarySpeedRequest request = item as SummarySpeedRequest;
      new TASNodeSpeedSummaryResult();

      bool success = this.raptorClient.GetSummarySpeed(request.projectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0,
          TASNodeCancellationDescriptorType.cdtVolumeSummary),
        RaptorConverters.ConvertFilter(request.FilterId, request.Filter, request.projectId, null, null,
          new List<long>()),
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
        out var result);

      if (success)
      {
        return ConvertResult(result);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested speed summary data"));
    }
  }
}
