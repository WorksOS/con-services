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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a PassCountDetailedResult if successful</returns>     
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      try
      {
        var request = item as PassCounts;

        if (request == null)
          ThrowRequestTypeCastException<PassCounts>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_PASSCOUNT"), out var useTrexGateway);

        if (useTrexGateway)
        {
          var pcDetailsRequest = new PassCountDetailsRequest(request.ProjectUid, request.Filter, request.passCountSettings.passCounts);
          return trexCompactionDataProxy.SendPassCountDetailsRequest(pcDetailsRequest, customHeaders).Result;
        }

        TPassCountDetails passCountDetails;

        var raptorFilter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId,
            request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds, log: log);
        var raptorResult = raptorClient.GetPassCountDetails(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.CallId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtPassCountDetailed),
          request.passCountSettings != null ? ConvertSettings(request.passCountSettings) : new TPassCountSettings(),
          raptorFilter,
          RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
          out passCountDetails);
        //log.LogDebug($"Result from Raptor {success} with {JsonConvert.SerializeObject(passCountDetails)}");

        if (raptorResult == TASNodeErrorStatus.asneOK)
            return ConvertResult(passCountDetails, request.liftBuildSettings);

        throw CreateServiceException<DetailedPassCountExecutor>((int)raptorResult);
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
  }
}
