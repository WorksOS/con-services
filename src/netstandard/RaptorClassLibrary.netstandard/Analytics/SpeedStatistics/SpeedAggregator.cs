using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.SpeedStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Speed summary
	/// </summary>
	public class SpeedAggregator : AggregatorBase
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
	  /// A value representing the count of cells that have reported machine speed values higher than a speed target.
	  /// </summary>
	  public long AboveTargetCellsCount { get; set; }
	  /// <summary>
	  /// A value representing the count of cells that have reported machine speed values lower than a speed target.
	  /// </summary>
	  public long BelowTargetCellsCount { get; set; }
	  /// <summary>
	  /// A value representing the count of cells that have reported machine speed values the same as a speed target.
	  /// </summary>
	  public long MatchTargetCellsCount { get; set; }
	  /// <summary>
	  /// The amount of production data the Speed statistics are requested for.
	  /// </summary>
	  public double CoverageArea { get; set; } // Area in sq/m...-

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
		  // Works out the percentage each colour on the map represents

		  if (!(subGrids[0][0] is ClientMachineSpeedLeafSubGrid SubGrid))
			  return;

		  SubGridUtilities.SubGridDimensionalIterator((I, J) =>
		  {
			  float Value = SubGrid.Cells[I, J];
			  if (Value != Consts.NullMachineSpeed) // is there a value to test
			  {
				  SummaryCellsScanned++;
				  if (Value > TargetMaxMachineSpeed)
					  AboveTargetCellsCount++;
				  else if (Value < TargetMinMachineSpeed)
					  BelowTargetCellsCount++;
					else
						MatchTargetCellsCount++;
				}
			});
	  }

	}
}
