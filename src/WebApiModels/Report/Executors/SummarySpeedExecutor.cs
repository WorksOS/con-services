using ASNode.SpeedSummary.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using System;
using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class SummarySpeedExecutor : RequestExecutorContainer
  {
    private SummarySpeedResult ConvertResult(TASNodeSpeedSummaryResult result)
    {

      return SummarySpeedResult.CreateSummarySpeedResult
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
      TASNodeSpeedSummaryResult result = new TASNodeSpeedSummaryResult();

      bool success = raptorClient.GetSummarySpeed(request.projectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
          TASNodeCancellationDescriptorType.cdtVolumeSummary),
        RaptorConverters.ConvertFilter(request.filterId, request.filter, request.projectId, null, null,
          new List<long>()),
        RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
        out result);

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