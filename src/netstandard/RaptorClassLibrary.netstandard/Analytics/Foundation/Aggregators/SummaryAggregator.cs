using VSS.TRex.Analytics.Aggregators;

namespace VSS.TRex.Analytics.Foundation.Aggregators
{
  /// <summary>
  /// Base class used by summary analytics aggregators supporting funcitons such as pass count summary, cut/fill summary, speed summary etc
  /// where the analytics are calculated at the cluster compute layer and reduced at the application service layer.
  /// </summary>
  public class SummaryAggregator : AggregatorBase
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

    /// <summary>
    /// Aggregator state is now single threaded in the context of processing subgrid
    /// information into it as the processing threads access independent substate aggregators which
    /// are aggregated together to form the final aggregation result. However, in contexts that do support
    /// threaded access to this sturcture the FRequiresSerialisation flag should be set
    /// </summary>
    public bool RequiresSerialisation { get; set; }

    public double ValueAtTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedAtTarget / SummaryCellsScanned * 100 : 0;

    public double ValueOverTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedOverTarget / SummaryCellsScanned * 100 : 0;

    public double ValueUnderTargetPercent => SummaryCellsScanned > 0 ? (double)CellsScannedUnderTarget / SummaryCellsScanned * 100 : 0;

    public double SummaryProcessedArea => SummaryCellsScanned * (CellSize * CellSize);

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public SummaryAggregator AggregateWith(SummaryAggregator other)
    {
      CellSize = other.CellSize;
      SummaryCellsScanned += other.SummaryCellsScanned;

      CellsScannedAtTarget += other.CellsScannedAtTarget;
      CellsScannedOverTarget += other.CellsScannedOverTarget;
      CellsScannedUnderTarget += other.CellsScannedUnderTarget;

      if (other.SummaryCellsScanned > 0)
      {
        IsTargetValueConstant &= other.IsTargetValueConstant;
        MissingTargetValue |= other.MissingTargetValue;
      }

      DataCheck(other);

      return this;
    }

    protected virtual void DataCheck(SummaryAggregator other)
    {
      // Nothing to implement...
    }
  }
}
