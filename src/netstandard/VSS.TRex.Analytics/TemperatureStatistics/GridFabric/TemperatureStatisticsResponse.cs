using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Temperature statistics request
	/// </summary>
	public class TemperatureStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<TemperatureStatisticsResponse>, IAnalyticsOperationResponseResultConversion<TemperatureStatisticsResult>
  {
		/// <summary>
		/// Holds last known good minimum temperature level value.
		/// </summary>
		public ushort LastTempRangeMin { get; set; }

		/// <summary>
		/// Holds last known good maximum temperature level value.
		/// </summary>
		public ushort LastTempRangeMax { get; set; }

    /// <summary>
    /// Aggregate a set of Temperature statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

			LastTempRangeMin = ((TemperatureStatisticsResponse) other).LastTempRangeMin;
			LastTempRangeMax = ((TemperatureStatisticsResponse) other).LastTempRangeMax;
		}

    public TemperatureStatisticsResult ConstructResult()
    {
      return new TemperatureStatisticsResult
      {
        MinimumTemperature = LastTempRangeMin,
        MaximumTemperature = LastTempRangeMax,
        IsTargetTemperatureConstant = IsTargetValueConstant,
        BelowTargetPercent = ValueUnderTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        AboveTargetPercent = ValueOverTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,
        Counts = Counts,

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoResult : MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.NoProblems,

        ResultStatus = ResultStatus
      };
    }

    public TemperatureStatisticsResponse AggregateWith(TemperatureStatisticsResponse other)
    {
      return base.AggregateWith(other) as TemperatureStatisticsResponse;
    }
  }
}
