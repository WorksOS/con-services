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
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the detailed pass counts request to Raptor
  /// </summary>
  public class DetailedPassCountExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedPassCountExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the detailed pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<PassCounts>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_PASSCOUNT"))
        {
#endif
          var pcDetailsRequest = new PassCountDetailsRequest(request.ProjectUid, request.Filter, request.passCountSettings.passCounts);
          return await trexCompactionDataProxy.SendDataPostRequest<PassCountDetailedResult, PassCountDetailsRequest>(pcDetailsRequest, "/passcounts/details", customHeaders);
#if RAPTOR
        }

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient, request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds);
        var raptorResult = raptorClient.GetPassCountDetails(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((request.CallId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtPassCountDetailed),
          request.passCountSettings != null ? ConvertSettings(request.passCountSettings) : new TPassCountSettings(),
          raptorFilter,
          RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
          out var passCountDetails);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(passCountDetails, request.liftBuildSettings);

        throw CreateServiceException<DetailedPassCountExecutor>((int)raptorResult);
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
    private PassCountDetailedResult ConvertResult(TPassCountDetails details, LiftBuildSettings liftSettings)
    {
      return new PassCountDetailedResult(
          ((liftSettings != null) && (liftSettings.OverridingTargetPassCountRange != null))
              ? liftSettings.OverridingTargetPassCountRange
              : (!details.IsTargetPassCountConstant ? new TargetPassCountRange(0, 0) : new TargetPassCountRange(details.ConstantTargetPassCountRange.Min, details.ConstantTargetPassCountRange.Max)),
          details.IsTargetPassCountConstant,
          details.Percents, details.TotalAreaCoveredSqMeters);
    }

    private TPassCountSettings ConvertSettings(PassCountSettings settings)
    {
      return new TPassCountSettings
      {
        IsSummary = false,
        PassCounts = settings.passCounts
      };
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
