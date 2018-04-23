using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
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
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Undefined requested data MDPRequest"));
        }

        string fileSpaceName = FileDescriptorExtensions.GetFileSpaceId(configStore, log);

        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
            request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds, fileSpaceName);
        bool success = raptorClient.GetMDPSummary(request.projectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtMDPSummary),
                            ConvertSettings(request.mdpSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out TMDPSummary mdpSummary);
        if (success)
        {
          result = ConvertResult(mdpSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested MDP summary data with error: {ContractExecutionStates.FirstNameWithOffset(mdpSummary.ReturnCode)}"));
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
      return MDPSummaryResult.Create(
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
        MDPTarget = settings.mdpTarget,
        IsSummary = true,
        MaxMDP = settings.maxMDP,
        MaxMDPPercent = settings.maxMDPPercent,
        MinMDP = settings.minMDP,
        MinMDPPercent = settings.minMDPPercent,
        OverrideTargetMDP = settings.overrideTargetMDP
      };
    }
  }
}
