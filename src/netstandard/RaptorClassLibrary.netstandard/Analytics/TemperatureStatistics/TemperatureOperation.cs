using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
  /// <summary>
  /// Provides a client consumable operation for performing temperature analytics that returns a client model space temperature result.
  /// </summary>
  public class TemperatureOperation : AnalyticsOperation<TemperatureStatisticsRequest_ApplicationService, TemperatureStatisticsArgument, TemperatureStatisticsResponse, TemperatureResult>
	{
		// ...
  }
}
