using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Common;
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
    private static byte VERSION_NUMBER = 1;

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
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteShort((short)LastTempRangeMin);
      writer.WriteShort((short)LastTempRangeMax);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        LastTempRangeMin = (ushort) reader.ReadShort();
        LastTempRangeMax = (ushort) reader.ReadShort();
      }
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
