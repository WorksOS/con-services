using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using ASNodeRPC;
using SVOICLiftBuildSettings;
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
  /// The executor which passes the detailed CMV request to Raptor
  /// </summary>
  public class DetailedCMVExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedCMVExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the detailed CMV request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CMVRequest>(item);
#if RAPTOR
        if (request.IsCustomCMVTargets && (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CMV") ?? false))
        {
#endif
          var settings = (CMVSettingsEx)request.CmvSettings;
          var cmvDetailsRequest = new CMVDetailsRequest(request.ProjectUid, request.Filter, settings.CustomCMVDetailTargets);
          return await trexCompactionDataProxy.SendDataPostRequest<CMVDetailedResult, CMVDetailsRequest>(cmvDetailsRequest, "/cmv/details", customHeaders);
#if RAPTOR
        }

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient, request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds);

        var externalRequestDescriptor = ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(
          request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtCMVDetailed);

        var liftBuildSettings = RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod);

        TCMVDetails cmvDetails;
        TASNodeErrorStatus raptorResult;

        if (!request.IsCustomCMVTargets)
        {
          raptorResult = raptorClient.GetCMVDetails(
            request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            externalRequestDescriptor,
            ConvertSettings(request.CmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }
        else
        {
          raptorResult = raptorClient.GetCMVDetailsExt(
            request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            externalRequestDescriptor,
            ConvertSettingsExt((CMVSettingsEx)request.CmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(cmvDetails);

        throw CreateServiceException<DetailedCMVExecutor>((int)raptorResult);
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
    private CMVDetailedResult ConvertResult(TCMVDetails details)
    {
      return new CMVDetailedResult(details.Percents);
    }

    private TCMVSettings ConvertSettings(CMVSettings settings)
    {
      return new TCMVSettings
      {
        CMVTarget = settings.CmvTarget,
        IsSummary = false,
        MaxCMV = settings.MaxCMV,
        MaxCMVPercent = settings.MaxCMVPercent,
        MinCMV = settings.MinCMV,
        MinCMVPercent = settings.MinCMVPercent,
        OverrideTargetCMV = settings.OverrideTargetCMV
      };
    }

    private TCMVSettingsExt ConvertSettingsExt(CMVSettingsEx settings)
    {
      return new TCMVSettingsExt()
      {
        CMVTarget = settings.CmvTarget,
        IsSummary = false,
        MaxCMV = settings.MaxCMV,
        MaxCMVPercent = settings.MaxCMVPercent,
        MinCMV = settings.MinCMV,
        MinCMVPercent = settings.MinCMVPercent,
        OverrideTargetCMV = settings.OverrideTargetCMV,
        CMVDetailPercents = settings.CustomCMVDetailTargets
      };
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
