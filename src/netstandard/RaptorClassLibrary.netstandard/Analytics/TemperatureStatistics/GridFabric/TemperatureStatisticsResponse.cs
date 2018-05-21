using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Temperature statistics request
	/// </summary>
	public class TemperatureStatisticsResponse : BaseAnalyticsResponse, IAggregateWith<TemperatureStatisticsResponse>
	{
		/// <summary>
		/// A value representing the count of cells that have reported temperature values higher than their maximum level.
		/// </summary>
		public long AboveTemperatureCellsCount { get; set; }

		/// <summary>
		/// A value representing the count of cells that have reported temperature values lower than their minimum level.
		/// </summary>
		public long BelowTemperatureCellsCount { get; set; }

		/// <summary>
		/// A value representing the count of cells that have reported temperature values are between their minimum and maximum levels.
		/// </summary>
		public long WithinTemperatureCellsCount { get; set; }

		/// <summary>
		/// The amount of production data the Temperature statistics are requested for.
		/// </summary>
		public double CoverageArea { get; set; } // Area in sq/m...-

		/// <summary>
		/// Aggregate a set of Speed statistics into this set and return the result.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>

		public TemperatureStatisticsResponse AggregateWith(TemperatureStatisticsResponse other)
		{
			AboveTemperatureCellsCount += other.AboveTemperatureCellsCount;
			BelowTemperatureCellsCount += other.BelowTemperatureCellsCount;
			WithinTemperatureCellsCount += other.WithinTemperatureCellsCount;
			CoverageArea += other.CoverageArea;

			return this;
		}
	}
}
