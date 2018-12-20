using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CCAStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a CCA statistics
  /// </summary>
  public class CCAStatisticsAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// Holds last known good target CMV value.
    /// </summary>
    public byte LastTargetCCA { get; private set; } = CellPassConsts.NullCCA;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CCAStatisticsAggregator()
    {
    }

    protected override int GetMaximumValue()
    {
      return CellPassConsts.NullCCA;
    }

    private bool CCACellValueToDisplay(byte measuredCCA, byte targetCCA, out double cellValue)
    {
      cellValue = CellPassConsts.NullCCA;

      if (measuredCCA == CellPassConsts.NullCCA)
        return false;

      if (targetCCA == CellPassConsts.NullCCATarget)
        return false;

      if (targetCCA != 0)
        cellValue = ((double)measuredCCA / targetCCA) * 100;

      return true;

      // TODO: When CCA details is to be implemented...
      //      case DisplayMode of
      //      icdmCCA: CellValue:= MeasuredCCA;

      //      icdmCCASummary:
      //      begin
      //      if int (TargetCCA) = int(kICNullCCATarget) then
      //        Exit;

      //      if TargetCCA <> 0 then
      //      CellValue := (MeasuredCCA / TargetCCA) * 100;
      //      end
      //      else
      ////    Assert(False); // Enable this assert if you want to be strict about it!!
      //      Exit;
      //      end;
    }

    /// <summary>
    /// Processes a CCA subgrid into a CCA isopach and calculate the counts of cells where the CCA value matches the requested target.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      base.ProcessSubgridResult(subGrids);

      // Loop through CCA array. Note processing is accumulative so values may already hold values.
      foreach (IClientLeafSubGrid[] subGrid in subGrids)
      {
        if (subGrid == null)
          continue;

        if (subGrid[0] is ClientCCALeafSubGrid SubGrid)
        {
          var currentTargetCCA = CellPassConsts.NullCCA;

          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
            var ccaValue = SubGrid.Cells[I, J];

            if (ccaValue.MeasuredCCA != CellPassConsts.NullCCA && ccaValue.MeasuredCCA < CellPassConsts.ThickLiftCCAValue) // Is there a measured value and not too thick to test?..
            {
              // Using the machine target values to check whether the target varies...
              if (IsTargetValueConstant) // Do we need to test...
              {
                if (ccaValue.TargetCCA != CellPassConsts.NullCCATarget && LastTargetCCA != CellPassConsts.NullCCATarget) // The values are all good to check..
                  IsTargetValueConstant = LastTargetCCA == ccaValue.TargetCCA;  // Check whether the target value varies...
              }

              if (LastTargetCCA != ccaValue.TargetCCA && ccaValue.TargetCCA != CellPassConsts.NullCCATarget)
                LastTargetCCA = ccaValue.TargetCCA; // Holds last valid target value...

              // Set the current target value...
              if (currentTargetCCA != ccaValue.TargetCCA)
                currentTargetCCA = ccaValue.TargetCCA;

              if (currentTargetCCA != CellPassConsts.NullCCATarget)
              {
                SummaryCellsScanned++;

                if (ccaValue.IsOvercompacted)
                  CellsScannedOverTarget++;
                else if (ccaValue.IsUndercompacted)
                  CellsScannedUnderTarget++;
                else
                  CellsScannedAtTarget++;
              }
              else
              {
                // We have data but no target data to do summary...
                MissingTargetValue = true; // Flag this to issue a warning to a user...
              }

              if (CCACellValueToDisplay(ccaValue.MeasuredCCA, LastTargetCCA, out var ccaPercentValue))
                IncrementCountOfTransition(ccaPercentValue); // CCA Summary is counted here...

              // TODO: When CCA details is to be implemented...
              //case ICDisplayMode of
              //icdmCCA:
              //Transitions.IncrementCountOfTransition(CCAValue.MeasuredCCA);

              //icdmCCASummary:
              //if CCACellValueToDisplay(CCAValue.MeasuredCCA, LastTargetCCA, ICDisplayMode, LiftBuildSettings, CCAPercentValue) then
              //Transitions.IncrementCountOfTransition(CCAPercentValue);
              //end;
            }
          });
        }
      }
    }

  }
}
