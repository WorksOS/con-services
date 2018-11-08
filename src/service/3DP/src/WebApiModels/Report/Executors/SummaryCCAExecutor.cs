using System;
using System.Net;
using ASNodeDecls;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary CCA request to Raptor
  /// </summary>
  public class SummaryCCAExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCCAExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary CCA request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a CCASummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        TCCASummary ccaSummary;
        var request = item as CCARequest;

        if (request == null)
          ThrowRequestTypeCastException<CCARequest>();

        var raptorFilter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);

        bool success = raptorClient.GetCCASummary(request.ProjectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtCCASummary),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
                            out ccaSummary);
         
        if (success)
          return ConvertResult(ccaSummary);

        throw CreateServiceException<SummaryCCAExecutor>(ccaSummary.ReturnCode);
      }

      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddMissingTargetDataResultMessages(ContractExecutionStates);
    }

    private CCASummaryResult ConvertResult(TCCASummary summary)
    {
      return CCASummaryResult.Create(
                summary.CompactedPercent,
                summary.OverCompactedPercent,
                summary.ReturnCode,
                summary.TotalAreaCoveredSqMeters,
                summary.UnderCompactedPercent);
    }
  }
}
