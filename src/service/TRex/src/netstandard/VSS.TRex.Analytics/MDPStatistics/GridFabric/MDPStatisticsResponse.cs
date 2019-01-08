using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a MDP statistics request
  /// </summary>
  public class MDPStatisticsResponse : StatisticsAnalyticsResponse, IAggregateWith<MDPStatisticsResponse>, 
    IAnalyticsOperationResponseResultConversion<MDPStatisticsResult>
  {
    /// <summary>
    /// Holds last known good target MDP value.
    /// </summary>
    public short LastTargetMDP { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteShort(LastTargetMDP);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      LastTargetMDP = reader.ReadShort();
    }

    /// <summary>
    /// Aggregate a set of MDP statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      LastTargetMDP = ((MDPStatisticsResponse)other).LastTargetMDP;

    }

    public MDPStatisticsResponse AggregateWith(MDPStatisticsResponse other)
    {
      return base.AggregateWith(other) as MDPStatisticsResponse;
    }


    public MDPStatisticsResult ConstructResult()
    {
      return new MDPStatisticsResult
      {
        IsTargetMDPConstant = IsTargetValueConstant,
        ConstantTargetMDP = IsTargetValueConstant ? LastTargetMDP : (short)-1,
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
