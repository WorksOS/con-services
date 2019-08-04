using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<MDPRequest>(item);
#if RAPTOR
        if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_MDP") ?? false)
        {
#endif
          var mdpSummaryRequest = new MDPSummaryRequest(
            request.ProjectUid.Value,
            request.Filter,
            request.MdpSettings.MdpTarget,
            request.MdpSettings.OverrideTargetMDP,
            request.MdpSettings.MaxMDPPercent,
            request.MdpSettings.MinMDPPercent,
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));

          return await trexCompactionDataProxy.SendDataPostRequest<MDPSummaryResult, MDPSummaryRequest>(mdpSummaryRequest, "/mdp/summary", customHeaders);
#if RAPTOR
        }

        string fileSpaceName = FileDescriptorExtensions.GetFileSpaceId(configStore, log);

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient, request.OverrideStartUtc, request.OverrideEndUtc, request.OverrideAssetIds, fileSpaceName);
        var raptorResult = raptorClient.GetMDPSummary(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((request.CallId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtMDPSummary),
          ConvertSettings(request.MdpSettings),
          raptorFilter,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
          out var mdpSummary);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(mdpSummary);

        throw CreateServiceException<SummaryMDPExecutor>((int)raptorResult);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }
#if RAPTOR
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
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
