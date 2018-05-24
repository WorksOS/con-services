using System;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Speed statistics request
	/// </summary>
  public class SpeedStatisticsResponse : BaseAnalyticsResponse, 
	  IAggregateWith<SpeedStatisticsResponse>, 
	  IAnalyticsOperationResponseResultConversion<SpeedResult>
	{
		/// <summary>
		/// A value representing the count of cells that have reported machine speed values higher than a speed target.
		/// </summary>
		public long AboveTargetCellsCount { get; set; }

		/// <summary>
		/// A value representing the count of cells that have reported machine speed values lower than a speed target.
		/// </summary>
		public long BelowTargetCellsCount { get; set; }

		/// <summary>
		/// A value representing the count of cells that have reported machine speed values the same as a speed target.
		/// </summary>
		public long MatchTargetCellsCount { get; set; }

		/// <summary>
		/// The amount of production data the Speed statistics are requested for.
		/// </summary>
		public double CoverageArea { get; set; } // Area in sq/m...-

		/// <summary>
		/// Aggregate a set of Speed statistics into this set and return the result.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public SpeedStatisticsResponse AggregateWith(SpeedStatisticsResponse other)
		{
			AboveTargetCellsCount += other.AboveTargetCellsCount;
			BelowTargetCellsCount += other.BelowTargetCellsCount;
			MatchTargetCellsCount += other.MatchTargetCellsCount;
			CoverageArea += other.CoverageArea;

			return this;
		}

    /// <summary>
    /// Construct the result for the speed statistics
    /// </summary>
    /// <returns></returns>
	  public SpeedResult ConstructResult()
	  {
	    throw new NotImplementedException();

	    return new SpeedResult
	    {
	    };
	  }
	}
}
