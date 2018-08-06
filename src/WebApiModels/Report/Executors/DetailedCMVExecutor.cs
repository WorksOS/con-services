using System;
using System.Net;
using ASNodeDecls;
using ASNodeRPC;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

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
      ContractExecutionResult result;

      try
      {
        CMVRequest request = item as CMVRequest;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId,
          request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);

        TASNodeRequestDescriptor externalRequestDescriptor = ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(
          request.callId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtCMVDetailed);

        TICLiftBuildSettings liftBuildSettings = RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod);

        TCMVDetails cmvDetails;
        TASNodeErrorStatus raptorResult;

        if (!request.isCustomCMVTargets)
        {
          raptorResult = raptorClient.GetCMVDetails(
            request.ProjectId ?? -1,
            externalRequestDescriptor,
            ConvertSettings(request.cmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }
        else
        {
          raptorResult = raptorClient.GetCMVDetailsExt(
            request.ProjectId ?? -1,
            externalRequestDescriptor,
            ConvertSettingsExt((CMVSettingsEx) request.cmvSettings),
            raptorFilter,
            liftBuildSettings,
            out cmvDetails);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = ConvertResult(cmvDetails);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested CMV details data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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

    private CMVDetailedResult ConvertResult(TCMVDetails details)
    {
      return CMVDetailedResult.Create(details.Percents);
    }

    private TCMVSettings ConvertSettings(CMVSettings settings)
    {
      return new TCMVSettings
      {
        CMVTarget = settings.cmvTarget,
        IsSummary = false,
        MaxCMV = settings.maxCMV,
        MaxCMVPercent = settings.maxCMVPercent,
        MinCMV = settings.minCMV,
        MinCMVPercent = settings.minCMVPercent,
        OverrideTargetCMV = settings.overrideTargetCMV
      };
    }
    private TCMVSettingsExt ConvertSettingsExt(CMVSettingsEx settings)
    {
      return new TCMVSettingsExt()
      {
        CMVTarget = settings.cmvTarget,
        IsSummary = false,
        MaxCMV = settings.maxCMV,
        MaxCMVPercent = settings.maxCMVPercent,
        MinCMV = settings.minCMV,
        MinCMVPercent = settings.minCMVPercent,
        OverrideTargetCMV = settings.overrideTargetCMV,
        CMVDetailPercents = settings.customCMVDetailTargets
      };
    }

  }
}
