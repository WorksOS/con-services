using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.Foundation.GridFabric.Responses
{
	/// <summary>
	/// Base class for summary analytics response.
	/// </summary>
  public class SummaryAnalyticsResponse : BaseAnalyticsResponse, IAggregateWith<SummaryAnalyticsResponse>
  {
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
    /// /// Aggregate a set of summary data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public SummaryAnalyticsResponse AggregateWith(SummaryAnalyticsResponse other)
    {
      AggregateBaseDataWith(other);

      return this;
    }

    /// <summary>
    /// Aggregate a set of generic data statistics into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void AggregateBaseDataWith(SummaryAnalyticsResponse other)
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
    }
  }
}
