using System;
using ASNodeDecls;
using VLPDDecls;
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
      try
      {
        var request = CastRequestObjectTo<MDPRequest>(item);
        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_MDP"), out var useTrexGateway);

        if (useTrexGateway)
        {
          var mdpSummaryRequest = new MDPSummaryRequest(
            request.ProjectUid,
            request.Filter,
            request.MdpSettings.MdpTarget,
            request.MdpSettings.OverrideTargetMDP,
            request.MdpSettings.MaxMDPPercent,
            request.MdpSettings.MinMDPPercent);

          return trexCompactionDataProxy.SendMDPSummaryRequest(mdpSummaryRequest, customHeaders).Result;
        }

        string fileSpaceName = FileDescriptorExtensions.GetFileSpaceId(configStore, log);

        var raptorFilter = RaptorConverters.ConvertFilter(request.FilterId, request.Filter, request.ProjectId,
          request.OverrideStartUtc, request.OverrideEndUtc, request.OverrideAssetIds, fileSpaceName);
        var raptorResult = raptorClient.GetMDPSummary(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtMDPSummary),
          ConvertSettings(request.MdpSettings),
          raptorFilter,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
          out var mdpSummary);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(mdpSummary);

        throw CreateServiceException<SummaryMDPExecutor>((int)raptorResult);
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
