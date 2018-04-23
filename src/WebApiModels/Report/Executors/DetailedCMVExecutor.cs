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
    /// Processes the detailed CMV request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      CMVRequest request = item as CMVRequest;
      TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
        request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);

      TASNodeRequestDescriptor externalRequestDescriptor = ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(
        request.callId ?? Guid.NewGuid(), 0,
        TASNodeCancellationDescriptorType.cdtCMVDetailed);

      TICLiftBuildSettings liftBuildSettings = RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod);

      TCMVDetails cmvDetails;
      bool success;

      if (!request.isCustomCMVTargets)
      {
        success = raptorClient.GetCMVDetails(
          request.projectId ?? -1,
          externalRequestDescriptor,
          ConvertSettings(request.cmvSettings),
          raptorFilter,
          liftBuildSettings,
          out cmvDetails);
      }
      else
      {
        success = raptorClient.GetCMVDetailsExt(
          request.projectId ?? -1,
          externalRequestDescriptor,
          ConvertSettingsExt((CMVSettingsEx) request.cmvSettings),
          raptorFilter,
          liftBuildSettings,
          out cmvDetails);
      }

      if (success)
      {
        result = ConvertResult(cmvDetails);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested CMV details data"));
      }

      return result;
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