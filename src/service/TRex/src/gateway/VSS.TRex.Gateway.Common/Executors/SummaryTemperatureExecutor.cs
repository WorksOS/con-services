using System;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get material temperature summary.
  /// </summary>
  public class SummaryTemperatureExecutor : BaseExecutor
  {
    private const ushort MIN_TEMPERATURE = 0;
    private const ushort MAX_TEMPERATURE = 4095;
    private const ushort TEMPERATURE_CONVERSION_FACTOR = 10;

    public SummaryTemperatureExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryTemperatureExecutor()
    {
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TemperatureSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<TemperatureSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new TemperatureStatisticsOperation();
      var temperatureSettings = request.Overrides?.TemperatureSettings;
      var temperatureSummaryResult = operation.Execute(
        new TemperatureStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverrideTemperatureWarningLevels = temperatureSettings != null && temperatureSettings.OverrideTemperatureRange,
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(
            temperatureSettings != null ? Convert.ToUInt16(temperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR) : MIN_TEMPERATURE,
            temperatureSettings != null ? Convert.ToUInt16(temperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR) : MAX_TEMPERATURE)
        }
      );

      if (temperatureSummaryResult != null)
      {
        if (temperatureSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(temperatureSummaryResult);

        throw CreateServiceException<SummaryTemperatureExecutor>(temperatureSummaryResult.ResultStatus);
      }

      throw CreateServiceException<SummaryTemperatureExecutor>();
    }

    private TemperatureSummaryResult ConvertResult(TemperatureStatisticsResult summary)
    {
      return new TemperatureSummaryResult(
        new TemperatureTargetData()
        {
          MinTemperatureMachineTarget = summary.MinimumTemperature,
          MaxTemperatureMachineTarget = summary.MaximumTemperature,
          TargetVaries = !summary.IsTargetTemperatureConstant
        }, 
        (short) summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.AboveTargetPercent,
        summary.WithinTargetPercent,
        summary.BelowTargetPercent);
    }
  }
}
