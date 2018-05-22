using VSS.TRex.Analytics.Models;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
  public class TemperatureResult : AnalyticsResult<TemperatureStatisticsResponse>
	{
		/// <summary>
		/// If the temperature value is constant, this is the minimum constant value of all temperature targets in the processed data.
		/// </summary>
		public double MinimumTemperature { get; private set; }

		/// <summary>
		/// If the temperature value is constant, this is the maximum constant value of all temperature targets in the processed data.
		/// </summary>
		public double MaximumTemperature { get; private set; }

		/// <summary>
		/// Are the temperature target values applying to all processed cells constant?
		/// </summary>
		public bool IsTargetTemperatureConstant { get; private set; }

		/// <summary>
		/// The percentage of the cells that are below the temperature range
		/// </summary>
		public double BelowTemperaturePercent { get; private set; }

		/// <summary>
		/// The percentage of cells that are within the target range
		/// </summary>
		public double WithinTemperaturePercent { get; private set; }

		/// <summary>
		/// The percentage of the cells that are above the temperature range
		/// </summary>
		public double AboveTemperaturePercent { get; private set; }

		/// <summary>
		/// The internal result code of the request. Documented elsewhere.
		/// </summary>
		public short ReturnCode { get; private set; }

		/// <summary>
		/// The total area covered by non-null cells in the request area
		/// </summary>
		public double TotalAreaCoveredSqMeters { get; private set; }
		
		/// <summary>
		///  Takes a response from the cluster compuet layer and transforms it into the model to be handed back to the client context
		/// </summary>
		/// <param name="response"></param>
		public override void PopulateFromClusterComputeResponse(TemperatureStatisticsResponse response)
		{
			MinimumTemperature = response.LastTempRangeMin;
			MaximumTemperature = response.LastTempRangeMax;
			IsTargetTemperatureConstant = response.IsTargetValueConstant;
			BelowTemperaturePercent = response.ValueUnderTargetPercent;
			WithinTemperaturePercent = response.ValueAtTargetPercent;
			AboveTemperaturePercent = response.ValueOverTargetPercent;
			TotalAreaCoveredSqMeters = response.SummaryProcessedArea;

			if (response.MissingTargetValue)
			{
				ReturnCode = response.SummaryCellsScanned == 0 ? 
					(short)1 : // No result due to missing target data...
					(short)2;  // Partial result due to missing target data...
			}
			else
				ReturnCode = 0; // No problems due to missing target data could still be no data however... 

			ResultStatus = response.ResultStatus;
		}
  }
}
