using System.Diagnostics;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.Foundation.GridFabric.Responses
{
  /// <summary>
  /// Base class for statistic analytics response.
  /// </summary>
  public class StatisticsAnalyticsResponse : BaseAnalyticsResponse, IAggregateWith<StatisticsAnalyticsResponse>
  {
    /// <summary>
    /// An array values representing the counts of cells within each of the details bands defined in the request.
    /// The array's size is the same as the number of the data details bands. For Cut/Fill data it is always 7.
    /// </summary>
    public long[] Counts { get; set; }

    /// <summary>
    /// The cell size of the site model the aggregation is being performed over
    /// </summary>
    public double CellSize { get; set; }

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
    /// Aggregate a set of data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public StatisticsAnalyticsResponse AggregateWith(StatisticsAnalyticsResponse other)
    {
      AggregateBaseDataWith(other);

      return this;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void AggregateBaseDataWith(StatisticsAnalyticsResponse other)
    {
      // Details...
      if (Counts != null && other.Counts != null)
      {
        Counts = Counts ?? new long[other.Counts.Length];

        Debug.Assert(Counts.Length == other.Counts.Length);

        for (int i = 0; i < Counts.Length; i++)
          Counts[i] += other.Counts[i];
      }

      // Summary...
      CellSize = other.CellSize;
      SummaryCellsScanned += other.SummaryCellsScanned;

      CellsScannedOverTarget += other.CellsScannedOverTarget;
      CellsScannedUnderTarget += other.CellsScannedUnderTarget;
      CellsScannedAtTarget += other.CellsScannedAtTarget;

      if (other.SummaryCellsScanned > 0)
      {
        IsTargetValueConstant &= other.IsTargetValueConstant;
        MissingTargetValue |= other.MissingTargetValue;
      }
    }
  }
}
