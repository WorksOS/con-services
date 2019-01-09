using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a CMV statistics request
  /// </summary>
  public class CMVStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<CMVStatisticsResponse>, 
    IAnalyticsOperationResponseResultConversion<CMVStatisticsResult>
  {
    /// <summary>
    /// Holds last known good target CMV value.
    /// </summary>
    public short LastTargetCMV { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteShort(LastTargetCMV);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      LastTargetCMV = reader.ReadShort();
    }

    /// <summary>
    /// Aggregate a set of CMV summary into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetCMV = ((CMVStatisticsResponse)other).LastTargetCMV;
    }

    public CMVStatisticsResponse AggregateWith(CMVStatisticsResponse other)
    {
      return base.AggregateWith(other) as CMVStatisticsResponse;
    }

    public CMVStatisticsResult ConstructResult()
    {
      return new CMVStatisticsResult
      {
        IsTargetCMVConstant = IsTargetValueConstant,
        ConstantTargetCMV = IsTargetValueConstant ? LastTargetCMV : (short)-1,
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
