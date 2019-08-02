using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as TemperatureSummaryRequest;

      if (request == null)
        ThrowRequestTypeCastException<TemperatureSummaryRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      var operation = new TemperatureStatisticsOperation();
      var temperatureSummaryResult = await operation.ExecuteAsync(
        new TemperatureStatisticsArgument()
        {
          ProjectID = siteModel.ID,
          Filters = new FilterSet(filter),
          Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
          LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
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

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
