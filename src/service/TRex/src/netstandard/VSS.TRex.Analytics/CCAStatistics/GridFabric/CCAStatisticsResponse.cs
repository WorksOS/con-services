using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CCAStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CCA statistics request
  /// </summary>
  public class CCAStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CCAStatisticsResponse>,
    IAnalyticsOperationResponseResultConversion<CCAStatisticsResult>
  {
    /// <summary>
    /// Holds last known good target CCA value.
    /// </summary>
    public byte LastTargetCCA { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteByte(LastTargetCCA);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      LastTargetCCA = reader.ReadByte();
    }

    /// <summary>
    /// Aggregate a set of CCA summary into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetCCA = ((CCAStatisticsResponse)other).LastTargetCCA;
    }

    public CCAStatisticsResponse AggregateWith(CCAStatisticsResponse other)
    {
      return base.AggregateWith(other) as CCAStatisticsResponse;
    }

    public CCAStatisticsResult ConstructResult()
    {
      return new CCAStatisticsResult
      {
        IsTargetCCAConstant = IsTargetValueConstant,
        ConstantTargetCCA = LastTargetCCA,
        AboveTargetPercent = ValueOverTargetPercent,
        WithinTargetPercent = ValueAtTargetPercent,
        BelowTargetPercent = ValueUnderTargetPercent,
        TotalAreaCoveredSqMeters = SummaryProcessedArea,

        ReturnCode = MissingTargetValue ? SummaryCellsScanned == 0 ? MissingTargetDataResultType.NoResult : MissingTargetDataResultType.PartialResult : MissingTargetDataResultType.NoProblems,

        Counts = Counts,
        ResultStatus = ResultStatus
      };
    }
  }
}
