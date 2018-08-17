using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Speed summary
	/// </summary>
	public class SpeedAggregator : SummaryDataAggregator
	{
	  /// <summary>
	  /// Machine speed target record. It contains min/max machine speed target value.
	  /// </summary>
	  public MachineSpeedExtendedRecord TargetMachineSpeed;

	  /// <summary>
	  /// Default no-arg constructor
	  /// </summary>
	  public SpeedAggregator()
	  {
			TargetMachineSpeed.Clear();
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
			  if (SpeedRangeValue.Max != CellPassConsts.NullMachineSpeed) // is there a value to test
			  {
				  SummaryCellsScanned++;
				  if (SpeedRangeValue.Max > TargetMachineSpeed.Max)
					  CellsScannedOverTarget++;
				  else if (SpeedRangeValue.Min < TargetMachineSpeed.Min && SpeedRangeValue.Max < TargetMachineSpeed.Min)
					  CellsScannedUnderTarget++;
					else
						CellsScannedAtTarget++;
				}
			});
	  }

	}
}
