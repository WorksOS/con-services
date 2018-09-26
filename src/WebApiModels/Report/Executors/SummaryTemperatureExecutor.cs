using System;
using System.Net;
using ASNodeDecls;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
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
      ContractExecutionResult result;
      try
      {
        TemperatureRequest request = item as TemperatureRequest;

        if (request == null)
          ThrowRequestTypeCastException(typeof(TemperatureRequest));

        TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId,
            request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
        var raptorResult = raptorClient.GetTemperatureSummary(request.ProjectId ?? -1,
                            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0, TASNodeCancellationDescriptorType.cdtTemperature),
                            ConvertSettings(request.temperatureSettings),
                            raptorFilter,
                            RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                            out var temperatureSummary);
        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          result = ConvertResult(temperatureSummary);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult((int)raptorResult,//ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to get requested Temperature summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
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

    private TemperatureSummaryResult ConvertResult(TTemperature summary)
    {
      return new TemperatureSummaryResult(
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
        MaximumTemperatureRange = settings.MaxTemperature,
        MinimumTemperatureRange = settings.MinTemperature,
        OverrideMachineTarget = settings.OverrideTemperatureRange
      };
    }

  }
}
