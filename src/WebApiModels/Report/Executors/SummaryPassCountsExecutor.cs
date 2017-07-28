using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using System;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor
  /// </summary>
  public class SummaryPassCountsExecutor : RequestExecutorContainer
  {
      /// <summary>
      /// This constructor allows us to mock raptorClient
      /// </summary>
      /// <param name="raptorClient"></param>
      public SummaryPassCountsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
      {
      }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryPassCountsExecutor()
    {
    }

    /// <summary>
    /// Processes the summary pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a PassCountSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        TPassCountSummary passCountSummary;
        PassCounts request = item as PassCounts;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
            request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
        bool success = raptorClient.GetPassCountSummary(request.projectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtPassCountSummary),
                            ConvertSettings(),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out passCountSummary);
        if (success)
        {
          result = ConvertResult(passCountSummary, request.liftBuildSettings);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Failed to get requested pass count summary data"));
        }
      }
 
      finally
      {
        //ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

      protected override void ProcessErrorCodes()
      {
          //TODO: once we have error codes for pass counts from Raptor
      }

      private PassCountSummaryResult ConvertResult(TPassCountSummary summary, LiftBuildSettings liftSettings)
    {
      return PassCountSummaryResult.CreatePassCountSummaryResult(
          ((liftSettings != null) && (liftSettings.overridingTargetPassCountRange != null)) ? liftSettings.overridingTargetPassCountRange : TargetPassCountRange.CreateTargetPassCountRange(summary.ConstantTargetPassCountRange.Min, summary.ConstantTargetPassCountRange.Max), 
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
        PassCounts = new int[]{0,0}
      };
    }
    
  }
}