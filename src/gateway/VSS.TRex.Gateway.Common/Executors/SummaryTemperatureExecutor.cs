using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Requests;
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
      TemperatureSummaryRequest request = item as TemperatureSummaryRequest;

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.filter, siteModel);

      TemperatureOperation operation = new TemperatureOperation();
      TemperatureResult temperatureSummaryResult = operation.Execute(
        new TemperatureStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverrideTemperatureWarningLevels = request.temperatureSettings != null && request.temperatureSettings.overrideTemperatureRange,
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(
            request.temperatureSettings != null ? Convert.ToUInt16(request.temperatureSettings.minTemperature * TEMPERATURE_CONVERSION_FACTOR) : MIN_TEMPERATURE,
            request.temperatureSettings != null ? Convert.ToUInt16(request.temperatureSettings.maxTemperature * TEMPERATURE_CONVERSION_FACTOR) : MAX_TEMPERATURE)
        }
      );

      if (temperatureSummaryResult != null)
        return ConvertResult(temperatureSummaryResult);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested material temperature summary data"));
    }

    private TemperatureSummaryResult ConvertResult(TemperatureResult summary)
    {
      return new TemperatureSummaryResult(
        summary.MinimumTemperature,
        summary.MaximumTemperature,
        summary.IsTargetTemperatureConstant,
        (short) summary.ReturnCode,
        summary.TotalAreaCoveredSqMeters,
        summary.AboveTargetPercent,
        summary.WithinTargetPercent,
        summary.BelowTargetPercent);
    }
  }
}
