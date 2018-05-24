using VSS.TRex.Analytics.Models;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
  public class TemperatureResult : AnalyticsResult
	{
		/// <summary>
		/// If the temperature value is constant, this is the minimum constant value of all temperature targets in the processed data.
		/// </summary>
		public double MinimumTemperature { get; set; }

		/// <summary>
		/// If the temperature value is constant, this is the maximum constant value of all temperature targets in the processed data.
		/// </summary>
		public double MaximumTemperature { get; set; }

		/// <summary>
		/// Are the temperature target values applying to all processed cells constant?
		/// </summary>
		public bool IsTargetTemperatureConstant { get; set; }

		/// <summary>
		/// The percentage of the cells that are below the temperature range
		/// </summary>
		public double BelowTemperaturePercent { get; set; }

		/// <summary>
		/// The percentage of cells that are within the target range
		/// </summary>
		public double WithinTemperaturePercent { get; set; }

		/// <summary>
		/// The percentage of the cells that are above the temperature range
		/// </summary>
		public double AboveTemperaturePercent { get; set; }

		/// <summary>
		/// The internal result code of the request. Documented elsewhere.
		/// </summary>
		public short ReturnCode { get; set; }

		/// <summary>
		/// The total area covered by non-null cells in the request area
		/// </summary>
		public double TotalAreaCoveredSqMeters { get; set; }
  }
}
