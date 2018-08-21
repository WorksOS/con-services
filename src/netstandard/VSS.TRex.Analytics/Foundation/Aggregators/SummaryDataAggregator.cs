
namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by summary analytics aggregators supporting funcitons such as pass count summary, speed summary etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class SummaryDataAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// The number of cells scanned while summarising information in the resulting analytics, report or export
    /// </summary>
    public int SummaryCellsScanned { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was in the target value range
    /// </summary>
    public int CellsScannedAtTarget { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was over the target value range
    /// </summary>
    public int CellsScannedOverTarget { get; set; }

    /// <summary>
    /// The number of cells scanned where the value from the cell was below the target value range
    /// </summary>
    public int CellsScannedUnderTarget { get; set; }

    /// <summary>
    /// Were the target values for all data extraqted for the analytics requested the same
    /// </summary>
    public bool IsTargetValueConstant { get; set; } = true;

    /// <summary>
    /// Were there any missing target values within the data extracted for the analytics request
    /// </summary>
    public bool MissingTargetValue { get; set; }

    public double ValueAtTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedAtTarget / SummaryCellsScanned * 100 : 0;

    public double ValueOverTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedOverTarget / SummaryCellsScanned * 100 : 0;

    public double ValueUnderTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedUnderTarget / SummaryCellsScanned * 100 : 0;

    public double SummaryProcessedArea => SummaryCellsScanned * (CellSize * CellSize);

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public SummaryDataAggregator AggregateWith(SummaryDataAggregator other)
    {
      return base.AggregateWith(other) as SummaryDataAggregator;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set.
    /// </summary>
    /// <param name="other"></param>
    protected override void AggregateBaseDataWith(DataStatisticsAggregator other)
    {
      base.AggregateBaseDataWith(other);

      var otherAggregator = (SummaryDataAggregator)other;

      SummaryCellsScanned += otherAggregator.SummaryCellsScanned;

      CellsScannedAtTarget += otherAggregator.CellsScannedAtTarget;
      CellsScannedOverTarget += otherAggregator.CellsScannedOverTarget;
      CellsScannedUnderTarget += otherAggregator.CellsScannedUnderTarget;

      if (otherAggregator.SummaryCellsScanned > 0)
      {
        IsTargetValueConstant &= otherAggregator.IsTargetValueConstant;
        MissingTargetValue |= otherAggregator.MissingTargetValue;
      }
    }

  }
}
