﻿using System;
using System.Net;
using ASNodeDecls;
using SVOICFilterSettings;
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
  /// The executor which passes the summary pass counts request to Raptor
  /// </summary>
  public class SummaryPassCountsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryPassCountsExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a PassCountSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        PassCounts request = item as PassCounts;

        if (request == null)
          ThrowRequestTypeCastException(typeof(PassCounts));

        if (!bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_PASSCOUNT"), out var useTrexGateway))
          useTrexGateway = false;

        if (useTrexGateway)
        {
          var pcSummaryRequest = new PassCountSummaryRequest(
            request.ProjectUid,
            request.Filter,
            request.liftBuildSettings.OverridingTargetPassCountRange);

          return trexCompactionDataProxy.SendPassCountSummaryRequest(pcSummaryRequest, customHeaders).Result;
        }

        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId,
          request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds);

        var raptorResult = raptorClient.GetPassCountSummary(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtPassCountSummary),
          ConvertSettings(),
          raptorFilter,
          RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
          out TPassCountSummary passCountSummary);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(passCountSummary, request.liftBuildSettings);

        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
          $"Failed to get requested pass count summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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

    private PassCountSummaryResult ConvertResult(TPassCountSummary summary, LiftBuildSettings liftSettings)
    {
      return new PassCountSummaryResult(
          liftSettings != null && liftSettings.OverridingTargetPassCountRange != null ? liftSettings.OverridingTargetPassCountRange : new TargetPassCountRange(summary.ConstantTargetPassCountRange.Min, summary.ConstantTargetPassCountRange.Max), 
          summary.IsTargetPassCountConstant, 
          summary.PercentEqualsTarget,
          summary.PercentGreaterThanTarget,
          summary.PercentLessThanTarget, 
          summary.ReturnCode, 
          summary.TotalAreaCoveredSqMeters);
    }

    private TPassCountSettings ConvertSettings()
    {
      return new TPassCountSettings
      {
        IsSummary = true,
        PassCounts = new[]{0,0}
      };
    }
  }
}
