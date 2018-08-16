using System;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a MDP summary and details
  /// </summary>
  public class MDPAggregator : SummaryDataAggregator
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine MDP target to be user overrides.
    /// </summary>
    public bool OverrideMachineMDP { get; set; }

    /// <summary>
    /// User overriding MDP target value.
    /// </summary>
    public short OverridingMachineMDP;

    /// <summary>
    /// MDP percentage range.
    /// </summary>
    public MDPRangePercentageRecord MDPPercentageRange;

    /// <summary>
    /// Holds last known good target MDP value.
    /// </summary>
    public short LastTargetMDP { get; private set; } = CellPassConsts.NullMDP;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public MDPAggregator()
    {
      OverridingMachineMDP = CellPassConsts.NullMDP;
      MDPPercentageRange.Clear();
    }

    protected override void DataCheck(DataStatisticsAggregator other)
    {
      var aggregator = (MDPAggregator) other;

      if (IsTargetValueConstant && aggregator.SummaryCellsScanned > 0) // if we need to check for a difference
      {
        // compare grouped results to determine if target varies
        if (aggregator.LastTargetMDP != CellPassConsts.NullMDP && LastTargetMDP != CellPassConsts.NullMDP) // if data valid
        {
          if (LastTargetMDP != aggregator.LastTargetMDP) // compare
            IsTargetValueConstant = false;
        }

        if (aggregator.LastTargetMDP != CellPassConsts.NullMDP) // if data valid
          LastTargetMDP = aggregator.LastTargetMDP; // set value
      }
    }

    /// <summary>
    /// Processes a MDP subgrid into a MDP isopach and calculate the counts of cells where the MDP value matches the requested target.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      base.ProcessSubgridResult(subGrids);

      // Works out the percentage each colour on the map represents

      if (!(subGrids[0][0] is ClientMDPLeafSubGrid SubGrid))
        return;

      var currentTargetMDP = CellPassConsts.NullMDP;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        var mdpValue = SubGrid.Cells[I, J];

        if (mdpValue.MeasuredMDP != CellPassConsts.NullMDP) // Is there a measured value to test?..
        {
          if (OverrideMachineMDP) // Are we overriding target values?..
          {
            if (LastTargetMDP != OverridingMachineMDP)
              LastTargetMDP = OverridingMachineMDP;
            if (currentTargetMDP != OverridingMachineMDP)
              currentTargetMDP = OverridingMachineMDP;
          }
          else
          {
            // Using the machine target values test if target varies...
            if (IsTargetValueConstant && mdpValue.TargetMDP != CellPassConsts.NullMDP && LastTargetMDP != CellPassConsts.NullMDP) // Values a re all good to test...
              IsTargetValueConstant = LastTargetMDP == mdpValue.TargetMDP; // Check to see if target value varies...

            if (LastTargetMDP != mdpValue.TargetMDP && mdpValue.TargetMDP != CellPassConsts.NullMDP)
              LastTargetMDP = mdpValue.TargetMDP; // LastTargetMDP holds last good value...

            if (currentTargetMDP != mdpValue.TargetMDP) // Set current target value...
              currentTargetMDP = mdpValue.TargetMDP;
          }

          if (currentTargetMDP != CellPassConsts.NullMDP) // If target is not null then do statistics...
          {
            var mdpRangeMin = Math.Round(LastTargetMDP * MDPPercentageRange.Min / 100);
            var mdpRangeMax = Math.Round(LastTargetMDP * MDPPercentageRange.Max / 100);

            SummaryCellsScanned++;
            if (mdpValue.MeasuredMDP > mdpRangeMax)
              CellsScannedOverTarget++;
            else if (mdpValue.MeasuredMDP < mdpRangeMin)
              CellsScannedUnderTarget++;
            else
              CellsScannedAtTarget++;
          }
          else // We have data but no target data to do a summary...
            MissingTargetValue = true; // Flag to issue a warning to user...

          // TODO: When MDP details is to be implemented...
          //switch (SubGrid.GridDataType)
          //{
          //  case GridDataType.MDP:
          //    Transitions.IncrementCountOfTransition(mdpValue.MeasuredMDP);
          //    break;
          //  case GridDataType.MDPPercent:
          //    if MDPCellValueToDisplay(MDPValue.MeasuredMDP, LastTargetMDP, SubGrid.GridDataType, LiftBuildSettings, MDPPercentValue) then
          //    Transitions.IncrementCountOfTransition(MDPPercentValue);
          //    break;
          //}
        }
      });
    }
  }
}
