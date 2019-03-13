using System;
#if RAPTOR
using ASNodeDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary Temperature request to Raptor
  /// </summary>
  public class SummaryTemperatureExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryTemperatureExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary Temperature request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TemperatureSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TemperatureRequest>(item);
#if RAPTOR
        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_TEMPERATURE"), out var useTrexGateway);

        if (useTrexGateway)
        {
#endif
          var temperatureSummaryRequest = new TemperatureSummaryRequest(
            request.ProjectUid,
            request.Filter,
            request.TemperatureSettings);

          return trexCompactionDataProxy.SendDataPostRequest<TemperatureSummaryResult, TemperatureSummaryRequest>(temperatureSummaryRequest, "/temperature/summary", customHeaders).Result;
#if RAPTOR
        }

        var raptorFilter = RaptorConverters.ConvertFilter(request.Filter, request.OverrideStartUTC, request.OverrideEndUTC, request.OverrideAssetIds);

        var raptorResult = raptorClient.GetTemperatureSummary(request.ProjectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((request.CallId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtTemperature),
                            ConvertSettings(request.TemperatureSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
                            out var temperatureSummary);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(temperatureSummary);

        throw CreateServiceException<SummaryTemperatureExecutor>((int)raptorResult);
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
    private TemperatureSummaryResult ConvertResult(TTemperature summary)
    {
      return new TemperatureSummaryResult(
                new TemperatureTargetData()
                {
                  MinTemperatureMachineTarget = summary.MinimumTemperature,
                  MaxTemperatureMachineTarget = summary.MaximumTemperature,
                  TargetVaries = !summary.IsTargetTemperatureConstant
                },
                summary.ReturnCode,
                summary.TotalAreaCoveredSqMeters,
                summary.AboveTemperaturePercent,
                summary.WithinTemperaturePercent,
                summary.BelowTemperaturePercent);
    }

    private TTemperatureSettings ConvertSettings(TemperatureSettings settings)
    {
      return new TTemperatureSettings
      {
        MaximumTemperatureRange = settings.MaxTemperature,
        MinimumTemperatureRange = settings.MinTemperature,
        OverrideMachineTarget = settings.OverrideTemperatureRange
      };
    }
#endif
  }
}
