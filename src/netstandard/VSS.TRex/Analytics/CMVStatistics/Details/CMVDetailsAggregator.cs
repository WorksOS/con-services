using System.Diagnostics;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.CMVStatistics.Details
{
  /// <summary>
  /// Implements the specific business rules for calculating a CMV details
  /// </summary>
  public class CMVDetailsAggregator : DetailsDataAggregator
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CMVDetailsAggregator()
    {
      // ...
    }

    public void IncrementCountOfTransition(double cmvValue)
    {
      Debug.Assert(DetailsDataValues.Length == Counts.Length, "Invalid size of the Counts array.");

      for (int i = 0; i < DetailsDataValues.Length; i++)
      {
        var startTransitionValue = DetailsDataValues[i];
        var endTransitionValue = i < DetailsDataValues.Length - 1 ? DetailsDataValues[i + 1] : CellPassConsts.NullCCV;

        if (cmvValue >= startTransitionValue && cmvValue < endTransitionValue)
        {
          Counts[i]++;
          break;
        }
      }
    }

    /// <summary>
    /// Processes a CMV subgrid into a CMV isopach and calculate the counts of cells where the CMV value fits into the requested bands.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      base.ProcessSubgridResult(subGrids);

      // Works out the percentage each colour on the map represents

      if (!(subGrids[0][0] is ClientCMVLeafSubGrid SubGrid))
        return;

      SubGridUtilities.SubGridDimensionalIterator((I, J) =>
      {
        var cmvValue = SubGrid.Cells[I, J];

        if (cmvValue.MeasuredCMV != CellPassConsts.NullCCV) // Is there a measured value to test?..
          IncrementCountOfTransition(cmvValue.MeasuredCMV);
      });
    }
  }
}
