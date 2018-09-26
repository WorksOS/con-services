using System;
using System.Net;
using ASNodeDecls;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary MDP request to Raptor
  /// </summary>
  public class SummaryMDPExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryMDPExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary MDP request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a CMVSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        MDPRequest request = item as MDPRequest;

        if (request == null)
          ThrowRequestTypeCastException(typeof(MDPRequest));

        string fileSpaceName = FileDescriptorExtensions.GetFileSpaceId(configStore, log);

        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.FilterId, request.Filter, request.ProjectId,
            request.OverrideStartUtc, request.OverrideEndUtc, request.OverrideAssetIds, fileSpaceName);
        var raptorResult = raptorClient.GetMDPSummary(request.ProjectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtMDPSummary),
                            ConvertSettings(request.MdpSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
                            out TMDPSummary mdpSummary);
        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = ConvertResult(mdpSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int) raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested MDP summary data with error: {ContractExecutionStates.FirstNameWithOffset((int) raptorResult)}"));
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


    private MDPSummaryResult ConvertResult(TMDPSummary summary)
    {
      return new MDPSummaryResult(
        summary.CompactedPercent,
        summary.ConstantTargetMDP,
        summary.IsTargetMDPConstant,
        summary.OverCompactedPercent,
        summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.UnderCompactedPercent);
    }

    private TMDPSettings ConvertSettings(MDPSettings settings)
    {
      return new TMDPSettings
      {
        MDPTarget = settings.MdpTarget,
        IsSummary = true,
        MaxMDP = settings.MaxMDP,
        MaxMDPPercent = settings.MaxMDPPercent,
        MinMDP = settings.MinMDP,
        MinMDPPercent = settings.MinMDPPercent,
        OverrideTargetMDP = settings.OverrideTargetMDP
      };
    }
  }
}
