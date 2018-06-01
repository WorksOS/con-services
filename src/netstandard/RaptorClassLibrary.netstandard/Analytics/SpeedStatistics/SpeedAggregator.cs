using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Speed summary
	/// </summary>
	public class SpeedAggregator : SummaryAggregator
  {
	  /// <summary>
	  /// Maximum machine speed target value.
	  /// </summary>
	  public ushort TargetMaxMachineSpeed { get; set; }

	  /// <summary>
	  /// Minimum machine speed target value.
	  /// </summary>
	  public ushort TargetMinMachineSpeed { get; set; }

	  /// <summary>
	  /// Default no-arg constructor
	  /// </summary>
	  public SpeedAggregator()
	  {
			// ...
	  }

	  /// <summary>
	  /// Processes a Speed subgrid into a Speed isopach and calculate the counts of cells where the Speed value
	  /// fits into the requested bands, i.e. less than min target, between min and max targets, greater than max target
	  /// </summary>
	  /// <param name="subGrids"></param>
	  public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
	  {
			base.ProcessSubgridResult(subGrids);

			// Works out the percentage each colour on the map represents

			if (!(subGrids[0][0] is ClientMachineTargetSpeedLeafSubGrid SubGrid))
			  return;

			SubGridUtilities.SubGridDimensionalIterator((I, J) =>
		  {
			  var SpeedRangeValue = SubGrid.Cells[I, J];
			  if (SpeedRangeValue.Max != CellPass.NullMachineSpeed) // is there a value to test
			  {
				  SummaryCellsScanned++;
				  if (SpeedRangeValue.Max > TargetMaxMachineSpeed)
					  CellsScannedOverTarget++;
				  else if (SpeedRangeValue.Min < TargetMinMachineSpeed && SpeedRangeValue.Max < TargetMinMachineSpeed)
					  CellsScannedUnderTarget++;
					else
						CellsScannedAtTarget++;
				}
			});
	  }

	}
}
