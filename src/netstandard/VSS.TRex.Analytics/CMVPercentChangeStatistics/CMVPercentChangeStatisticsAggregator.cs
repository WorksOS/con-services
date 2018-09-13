using System.Diagnostics;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Analytics.CMVPercentChangeStatistics
{
  public class CMVPercentChangeStatisticsAggregator : DataStatisticsAggregator
  {
    /// <summary>
    /// Details data values.
    /// </summary>
    public double[] CMVPercentChangeDetailsDataValues { get; set; }

    protected override int GetMaximumValue()
    {
      return CellPassConsts.NullCCV;
    }

    /// <summary>
    /// Processes a CMV % change subgrid into a CMV isopach and calculate the counts of cells where the CMV value matches the requested target.
    /// </summary>
    /// <param name="subGrids"></param>
    public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
    {
      const double MAX_PERCENTAGE_VALUE = 100;

      base.ProcessSubgridResult(subGrids);

      // Works out the percentage each colour on the map represents

      foreach (IClientLeafSubGrid[] subGrid in subGrids)
      {
        if (subGrid == null)
          continue;

        if (subGrid[0] is ClientCMVLeafSubGrid SubGrid)
        {
          double cmvChangeValue;

          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
            var cmvValue = SubGrid.Cells[I, J];

            if (cmvValue.MeasuredCMV != CellPassConsts.NullCCV) // Is there a measured value to test?..
            {
              SummaryCellsScanned++;

              if (cmvValue.PreviousMeasuredCMV == CellPassConsts.NullCCV)
                cmvChangeValue = MAX_PERCENTAGE_VALUE;
              else
                cmvChangeValue = (cmvValue.MeasuredCMV - cmvValue.PreviousMeasuredCMV) / cmvValue.PreviousMeasuredCMV * MAX_PERCENTAGE_VALUE;

              IncrementCountOfTransition(cmvChangeValue); // CMV Change Detail is counted here...
            }
          });
        }
      }
    }

  }
}
