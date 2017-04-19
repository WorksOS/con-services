using System;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the detailed pass counts request to Raptor
  /// </summary>
  public class DetailedPassCountExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public DetailedPassCountExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedPassCountExecutor()
    {
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
          TPassCountDetails passCountDetails;
          PassCounts request = item as PassCounts;
          TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
              request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
          bool success = raptorClient.GetPassCountDetails(request.projectId ?? -1,
              ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                  TASNodeCancellationDescriptorType.cdtPassCountDetailed),
              request.passCountSettings != null ? ConvertSettings(request.passCountSettings) : new TPassCountSettings(),
              raptorFilter,
              RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
              out passCountDetails);
          if (success)
          {
            result = ConvertResult(passCountDetails, request.liftBuildSettings);
          }
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                "Failed to get requested pass count details data"));
          }
        }
        finally
        {
          //ContractExecutionStates.ClearDynamic();
        }
        return result;
    }

      protected override void ProcessErrorCodes()
      {
      }

      private PassCountDetailedResult ConvertResult(TPassCountDetails details, LiftBuildSettings liftSettings)
      {
        return PassCountDetailedResult.CreatePassCountDetailedResult(
            ((liftSettings != null) && (liftSettings.overridingTargetPassCountRange != null)) 
                ? liftSettings.overridingTargetPassCountRange 
                : (!details.IsTargetPassCountConstant ? TargetPassCountRange.CreateTargetPassCountRange((ushort)0, (ushort)0) : TargetPassCountRange.CreateTargetPassCountRange(details.ConstantTargetPassCountRange.Min, details.ConstantTargetPassCountRange.Max)),
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