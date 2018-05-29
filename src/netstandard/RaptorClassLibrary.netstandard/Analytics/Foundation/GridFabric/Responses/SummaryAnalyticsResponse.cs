using VSS.TRex.Analytics.GridFabric.Responses;

namespace VSS.TRex.Analytics.Foundation.GridFabric.Responses
{
	/// <summary>
	/// Base class for summary analytics response.
	/// </summary>
  public class SummaryAnalyticsResponse : BaseAnalyticsResponse
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
  }
}
