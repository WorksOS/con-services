using System.Diagnostics;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.PassCountStatistics.Details
{
  /// <summary>
  /// Implements the specific business rules for calculating a Pass Count details
  /// </summary>
  public class PassCountDetailsAggregator : DetailsDataAggregator
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public PassCountDetailsAggregator()
    {
      // ...
    }

    public void IncrementCountOfTransition(double passCountValue)
    {
      Debug.Assert(DetailsDataValues.Length == Counts.Length, "Invalid size of the Counts array.");

      for (int i = 0; i < DetailsDataValues.Length; i++)
      {
        var startTransitionValue = DetailsDataValues[i];
        var endTransitionValue = i < DetailsDataValues.Length - 1 ? DetailsDataValues[i + 1] : CellPassConsts.MaxPassCountValue;

        if (passCountValue >= startTransitionValue && passCountValue < endTransitionValue)
        {
          Counts[i]++;
          break;
        }
      }
    }

    /// <summary>
    /// Processes a Pass Count subgrid into a Pass Count isopach and calculate the counts of cells where the Pass Count value fits into the requested bands.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      base.ProcessSubgridResult(subGrids);

      // Works out the percentage each colour on the map represents

      if (!(subGrids[0][0] is ClientPassCountLeafSubGrid SubGrid))
        return;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        var passCountValue = SubGrid.Cells[I, J];

        if (passCountValue.MeasuredPassCount != CellPassConsts.NullPassCountValue) // Is there a measured value to test?..
          IncrementCountOfTransition(passCountValue.MeasuredPassCount);
      });
    }

  }
}
