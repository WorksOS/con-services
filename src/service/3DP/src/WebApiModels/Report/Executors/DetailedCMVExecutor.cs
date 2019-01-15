using System;
using ASNodeDecls;
using ASNodeRPC;
using SVOICLiftBuildSettings;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CMVRequest>(item);

        if (!request.IsCustomCMVTargets || !bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_CMV"), out var useTrexGateway))
          useTrexGateway = false;

        if (useTrexGateway)
        {
          var settings = (CMVSettingsEx) request.CmvSettings;
          var cmvDetailsRequest = new CMVDetailsRequest(request.ProjectUid, request.Filter, settings.CustomCMVDetailTargets);
          return trexCompactionDataProxy.SendCMVDetailsRequest(cmvDetailsRequest, customHeaders).Result;
        }

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds);

        TASNodeRequestDescriptor externalRequestDescriptor = ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(
          request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtCMVDetailed);

        TICLiftBuildSettings liftBuildSettings = RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod);

        TCMVDetails cmvDetails;
        TASNodeErrorStatus raptorResult;

        if (!request.IsCustomCMVTargets)
        {
          raptorResult = raptorClient.GetCMVDetails(
            request.ProjectId ?? -1,
            externalRequestDescriptor,
            ConvertSettings(request.CmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }
        else
        {
          raptorResult = raptorClient.GetCMVDetailsExt(
            request.ProjectId ?? -1,
            externalRequestDescriptor,
            ConvertSettingsExt((CMVSettingsEx)request.CmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(cmvDetails);

        throw CreateServiceException<DetailedCMVExecutor>((int)raptorResult);
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

  }
}
