using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
  /// <summary>
  /// The result obtained from performing a temperature analytics request
  /// </summary>
  public class TemperatureResult : SummaryAnalyticsResult
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
		/// The internal result code of the request. Documented elsewhere.
		/// </summary>
		public MissingTargetDataResultType ReturnCode { get; set; }
  }
}
