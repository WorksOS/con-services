using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Filters;
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
      const string ERROR_MESSAGE = "Failed to get requested material temperature summary data";

      TemperatureSummaryRequest request = item as TemperatureSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException(typeof(TemperatureSummaryRequest));

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      TemperatureStatisticsOperation operation = new TemperatureStatisticsOperation();
      TemperatureStatisticsResult temperatureSummaryResult = operation.Execute(
        new TemperatureStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          OverrideTemperatureWarningLevels = request.TemperatureSettings != null && request.TemperatureSettings.OverrideTemperatureRange,
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(
            request.TemperatureSettings != null ? Convert.ToUInt16(request.TemperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR) : MIN_TEMPERATURE,
            request.TemperatureSettings != null ? Convert.ToUInt16(request.TemperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR) : MAX_TEMPERATURE)
        }
      );

      if (temperatureSummaryResult != null)
      {
        if (temperatureSummaryResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(temperatureSummaryResult);

        throw CreateServiceException(ERROR_MESSAGE, temperatureSummaryResult.ResultStatus);
      }

      throw CreateServiceException(ERROR_MESSAGE);
    }

    private TemperatureSummaryResult ConvertResult(TemperatureStatisticsResult summary)
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
