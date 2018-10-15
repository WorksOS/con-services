using Microsoft.Extensions.Logging;
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
  /// Process request to get temperature details
  /// </summary>
  public class DetailedTemperatureExecutor : BaseExecutor
  {


    public DetailedTemperatureExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedTemperatureExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      TemperatureDetailRequest request = item as TemperatureDetailRequest;

      if (request == null)
        ThrowRequestTypeCastException<TemperatureDetailRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);

      TemperatureStatisticsOperation operation = new TemperatureStatisticsOperation();
      TemperatureStatisticsResult temperatureDetailResult = operation.Execute(new TemperatureStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        TemperatureDetailValues = request.TemperatureList
      });

      if (temperatureDetailResult != null)
      {
        if (temperatureDetailResult.ResultStatus == RequestErrorStatus.OK)
          return new TemperatureDetailResult(new TemperatureTargetData()
            {
              MaxTemperatureMachineTarget = temperatureDetailResult.MaximumTemperature,
              MinTemperatureMachineTarget = temperatureDetailResult.MinimumTemperature,
              TargetVaries = temperatureDetailResult.IsTargetTemperatureConstant
            }, 
            temperatureDetailResult.Percents
          );

        throw CreateServiceException<DetailedTemperatureExecutor>(temperatureDetailResult.ResultStatus);
      }

      throw CreateServiceException<DetailedTemperatureExecutor>();
    }
  }


}
