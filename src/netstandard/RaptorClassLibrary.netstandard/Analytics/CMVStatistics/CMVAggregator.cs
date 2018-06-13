using System;
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

      var currentTargetCMV = CellPass.NullCCV;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        var cmvValue = SubGrid.Cells[I, J];

        if (cmvValue.MeasuredCMV != CellPass.NullCCV) // Is there a measured value to test?..
        {
          if (OverrideMachineCMV) // Are we overriding target values?..
          {
            if (LastTargetCMV != OverridingMachineCMV)
              LastTargetCMV = OverridingMachineCMV;
            if (currentTargetCMV != OverridingMachineCMV)
              currentTargetCMV = OverridingMachineCMV;
          }
          else
          {
            // Using the machine target values test if target varies...
            if (IsTargetValueConstant && cmvValue.TargetCMV != CellPass.NullCCV && LastTargetCMV != CellPass.NullCCV) // Values a re all good to test...
              IsTargetValueConstant = LastTargetCMV == cmvValue.TargetCMV; // Check to see if target value varies...

            if (LastTargetCMV != cmvValue.TargetCMV && cmvValue.TargetCMV != CellPass.NullCCV)
              LastTargetCMV = cmvValue.TargetCMV; // LastTargetCMV holds last good value...

            if (currentTargetCMV != cmvValue.TargetCMV) // Set current target value...
              currentTargetCMV = cmvValue.TargetCMV;
          }

          if (currentTargetCMV != CellPass.NullCCV) // If target is not null then do statistics...
          { 
            var cmvRangeMin = Math.Round(LastTargetCMV * CMVPercentageRange.Min / 100);
            var cmvRangeMax = Math.Round(LastTargetCMV * CMVPercentageRange.Max / 100);

            SummaryCellsScanned++;
            if (cmvValue.MeasuredCMV > cmvRangeMax)
              CellsScannedOverTarget++;
            else if (cmvValue.MeasuredCMV < cmvRangeMin)
              CellsScannedUnderTarget++;
            else
              CellsScannedAtTarget++;
          }
          else // We have data but no target data to do a summary...
            MissingTargetValue = true; // Flag to issue a warning to user...

          // TODO: When CMV details is to be implemented...
          //switch (SubGrid.GridDataType)
          //{
          //  case GridDataType.CCV:
          //    Transitions.IncrementCountOfTransition(cmvValue.MeasuredCMV);
          //    break;
          //  case GridDataType.CCVPercent:
          //    if CCVCellValueToDisplay(cmvValue.MeasuredCMV, LastTargetCMV, SubGrid.GridDataType, LiftBuildSettings, CCVPercentValue) then
          //    Transitions.IncrementCountOfTransition(CCVPercentValue);
          //    break;
          //}
        }
      });
    }
  }
}
