using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a CMV summary and details
  /// </summary>
  public class CMVAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine CMV target to be user overrides.
    /// </summary>
    public bool OverrideMachineCMV { get; set; }

    /// <summary>
    /// User overriding CMV target value.
    /// </summary>
    public short OverridingMachineCMV;

    /// <summary>
    /// CMV percentage range.
    /// </summary>
    public CMVRangePercentageRecord CMVPercentageRange;

    /// <summary>
    /// Holds last known good target CMV value.
    /// </summary>
    public short LastTargetCMV { get; private set; } = CellPass.NullCCV;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CMVAggregator()
    {
      OverridingMachineCMV = CellPass.NullCCV;
      CMVPercentageRange.Clear();
    }

    protected override void DataCheck(DataStatisticsAggregator other)
    {
      var aggregator = (CMVAggregator) other;

      if (IsTargetValueConstant && other.SummaryCellsScanned > 0) // if we need to check for a difference
      {
        // compare grouped results to determine if target varies
        if (aggregator.LastTargetCMV != CellPass.NullCCV && LastTargetCMV != CellPass.NullCCV) // if data valid
        {
          if (LastTargetCMV != aggregator.LastTargetCMV) // compare
            IsTargetValueConstant = false;
        }

        if (aggregator.LastTargetCMV != CellPass.NullCCV) // if data valid
          LastTargetCMV = aggregator.LastTargetCMV; // set value
      }
    }

    /// <summary>
    /// Processes a CMV subgrid into a CMV isopach and calculate the counts of cells where the CMV value matches the requested target.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      base.ProcessSubgridResult(subGrids);

      // Works out the percentage each colour on the map represents

      if (!(subGrids[0][0] is ClientCMVLeafSubGrid SubGrid))
        return;

      var currentTargetCCV = CellPass.NullCCV;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {

      });
    }
  }
}
