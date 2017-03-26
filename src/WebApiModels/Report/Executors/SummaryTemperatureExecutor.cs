using System;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary Temperature request to Raptor
  /// </summary>
  public class SummaryTemperatureExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public SummaryTemperatureExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryTemperatureExecutor()
    {
    }

    /// <summary>
    /// Processes the summary Temperature request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TemperatureSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        TTemperature temperatureSummary;
        TemperatureRequest request = item as TemperatureRequest;
        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
            request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
        bool success = raptorClient.GetTemperatureSummary(request.projectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtTemperature),
                            ConvertSettings(request.temperatureSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out temperatureSummary);
        if (success)
        {
          result = ConvertResult(temperatureSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            string.Format("Failed to get requested Temperature summary data with error: {0}", ContractExecutionStates.FirstNameWithOffset(temperatureSummary.ReturnCode))));
        }

      }

      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }


    private TemperatureSummaryResult ConvertResult(TTemperature summary)
    {
      return TemperatureSummaryResult.CreateTemperatureSummaryResult(
                summary.MinimumTemperature,
                summary.MaximumTemperature,
                summary.IsTargetTemperatureConstant,
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
        MaximumTemperatureRange = settings.maxTemperature,
        MinimumTemperatureRange = settings.minTemperature,
        OverrideMachineTarget = settings.overrideTemperatureRange
      };
    }

  }
}
