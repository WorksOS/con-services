using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Speed statistics request
	/// </summary>
  public class SpeedStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<SpeedStatisticsResponse>, IAnalyticsOperationResponseResultConversion<SpeedStatisticsResult>
	{
		/// <summary>
		/// Aggregate a set of Speed statistics into this set and return the result.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public SpeedStatisticsResponse AggregateWith(SpeedStatisticsResponse other)
		{
			return base.AggregateWith(other) as SpeedStatisticsResponse;
		}

    /// <summary>
    /// Construct the result for the speed statistics
    /// </summary>
    /// <returns></returns>
	  public SpeedStatisticsResult ConstructResult()
	  {
	    return new SpeedStatisticsResult
	    {
	      BelowTargetPercent = ValueUnderTargetPercent,
	      WithinTargetPercent = ValueAtTargetPercent,
	      AboveTargetPercent = ValueOverTargetPercent,
	      TotalAreaCoveredSqMeters = SummaryProcessedArea,

	      ResultStatus = ResultStatus
      };
	  }
	}
}
