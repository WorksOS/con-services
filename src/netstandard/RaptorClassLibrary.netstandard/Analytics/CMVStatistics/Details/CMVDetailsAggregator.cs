using System.Diagnostics;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Analytics.CMVStatistics.Details
{
  /// <summary>
  /// Implements the specific business rules for calculating a CMV details
  /// </summary>
  public class CMVDetailsAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// CMV details values.
    /// </summary>
    public int[] CMVDetailValues { get; set; }

    /// <summary>
    /// An array values representing the counts of cells within each of the CMV details bands defined in the request.
    /// The array's size is the same as the number of the CMV details bands.
    /// </summary>
    public long[] Counts { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CMVDetailsAggregator()
    {
      Counts = new long[0];
    }

    public void IncrementCountOfTransition(double cmvValue)
    {
      Debug.Assert(CMVDetailValues.Length == Counts.Length, "Invalid size of the Counts array.");

      for (int i = 0; i < CMVDetailValues.Length; i++)
      {
        var startTransitionValue = CMVDetailValues[i];
        var endTransitionValue = i < CMVDetailValues.Length - 1 ? CMVDetailValues[i + 1] : CellPass.NullCCV;

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

        if (cmvValue.MeasuredCMV != CellPass.NullCCV) // Is there a measured value to test?..
          IncrementCountOfTransition(cmvValue.MeasuredCMV);
      });
    }
  }
}
