using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// The response state returned from a Temperature statistics request
	/// </summary>
	public class TemperatureStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<TemperatureStatisticsResponse>, 
	  IAnalyticsOperationResponseResultConversion<TemperatureStatisticsResult>
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
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteShort((short)LastTempRangeMin);
      writer.WriteShort((short)LastTempRangeMax);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      LastTempRangeMin = (ushort)reader.ReadShort();
      LastTempRangeMax = (ushort)reader.ReadShort();
    }

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
        MinimumTemperature = IsTargetValueConstant ? LastTempRangeMin : 0.0,
        MaximumTemperature = IsTargetValueConstant ? LastTempRangeMax : 0.0,
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
