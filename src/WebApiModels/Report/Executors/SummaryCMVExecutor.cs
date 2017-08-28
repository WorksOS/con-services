using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary CMV request to Raptor
  /// </summary>
  public class SummaryCMVExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryCMVExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary CMV request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a CMVSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        TCMVSummary cmvSummary;
        CMVRequest request = item as CMVRequest;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
            request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
        bool success = raptorClient.GetCMVSummary(request.projectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtCMVSummary),
                            ConvertSettings(request.cmvSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out cmvSummary);
        if (success)
        {
          result = ConvertResult(cmvSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            string.Format("Failed to get requested CMV summary data with error: {0}", ContractExecutionStates.FirstNameWithOffset(cmvSummary.ReturnCode))));
        }

      }

      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    private CMVSummaryResult ConvertResult(TCMVSummary summary)
    {
      return CMVSummaryResult.CreateCMVSummaryResult(
                summary.CompactedPercent,
                summary.ConstantTargetCMV,
                summary.IsTargetCMVConstant,
                summary.OverCompactedPercent,
                summary.ReturnCode,
                summary.TotalAreaCoveredSqMeters,
                summary.UnderCompactedPercent);
    }

    private TCMVSettings ConvertSettings(CMVSettings settings)
    {
      return new TCMVSettings
      {
        CMVTarget = settings.cmvTarget,
        IsSummary = true,
        MaxCMV = settings.maxCMV,
        MaxCMVPercent = settings.maxCMVPercent,
        MinCMV = settings.minCMV,
        MinCMVPercent = settings.minCMVPercent,
        OverrideTargetCMV = settings.overrideTargetCMV

      };
    }
  }
}