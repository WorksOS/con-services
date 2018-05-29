using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Temperature statistics request
	/// </summary>
	public class TemperatureStatisticsResponse : SummaryAnalyticsResponse, 
	  IAggregateWith<TemperatureStatisticsResponse>, 
	  IAnalyticsOperationResponseResultConversion<TemperatureResult>
  {
		/// <summary>
		/// Holds last known good minimum temperature level value.
		/// </summary>
		public ushort LastTempRangeMin { get; set; }

		/// <summary>
		/// Holds last known good maximum temperature level value.
		/// </summary>
		public ushort LastTempRangeMax { get; set; }


		///// <summary>
		///// The amount of production data the Temperature statistics are requested for.
		///// </summary>
		//  public double CoverageArea { get; set; } // Area in sq/m...-

		public double ValueAtTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedAtTarget / SummaryCellsScanned * 100 : 0;

	  public double ValueOverTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedOverTarget / SummaryCellsScanned * 100 : 0;

	  public double ValueUnderTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedUnderTarget / SummaryCellsScanned * 100 : 0;

	  public double SummaryProcessedArea => SummaryCellsScanned * (CellSize * CellSize);

    /// <summary>
    /// Aggregate a set of Speed statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public TemperatureStatisticsResponse AggregateWith(TemperatureStatisticsResponse other)
		{
		  CellSize = other.CellSize;
		  SummaryCellsScanned += other.SummaryCellsScanned;

		  CellsScannedOverTarget += other.CellsScannedOverTarget;
		  CellsScannedUnderTarget += other.CellsScannedUnderTarget;
		  CellsScannedAtTarget += other.CellsScannedAtTarget;
      //CoverageArea += other.CoverageArea;

		  if (other.SummaryCellsScanned > 0)
		  {
		    IsTargetValueConstant &= other.IsTargetValueConstant;
		    MissingTargetValue |= other.MissingTargetValue;
		  }

			LastTempRangeMin = other.LastTempRangeMin;
			LastTempRangeMax = other.LastTempRangeMax;

      return this;
		}

    public TemperatureResult ConstructResult()
    {
      return new TemperatureResult
      {
        MinimumTemperature = LastTempRangeMin,
        MaximumTemperature = LastTempRangeMax,
        IsTargetTemperatureConstant = IsTargetValueConstant,
        BelowTemperaturePercent = ValueUnderTargetPercent,
        WithinTemperaturePercent = ValueAtTargetPercent,
        AboveTemperaturePercent = ValueOverTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        // 0 : No problems due to missing target data could still be no data however... 
        // 1 : No result due to missing target data...
        // 2 : Partial result due to missing target data...

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? (short)1 : (short)2 : (short)0,

        ResultStatus = ResultStatus
      };
    }
  }
}
