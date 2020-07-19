using System;
using System.Threading;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Analytics.CutFillStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a cut fill summary
  /// </summary>
  public class CutFillStatisticsAggregator : DataStatisticsAggregator
  {
    private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private bool _disposedValue;

    /// <summary>
    /// The array of height offsets representing the cut and fill bands of the cut-fill isopac surface being analysed
    /// </summary>
    public double[] Offsets { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CutFillStatisticsAggregator()
    {
      Counts = new long[7];
    }

    /// <summary>
    /// Determines which cut/fill band to allocate the height value for a cell
    /// </summary>
    private void IncrementCountOfCutFillTransition(double value)
    {
      // Works out what percentage of cut/fill map colours are used
      // always 7 elements in array and assumes grade is set at zero
      // eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
      if (value == 0) // on grade
        Counts[3]++;
      else if (value > 0) // Cut
      {
        if (value >= Offsets[0])
        {
          Counts[0]++;
          return;
        }

        for (int I = 0; I < 3; I++)
        {
          if (value >= Offsets[I + 1] && value < Offsets[I])
          {
            Counts[I + 1]++;
            break;
          }
        }

        // should not get past this point
      }
      else // must be fill
      {
        if (value <= Offsets[6])
        {
          Counts[6]++;
          return;
        }

        for (int I = 3; I < 6; I++)
        {
          if (value >= Offsets[I + 1] && value < Offsets[I])
          {
            Counts[I]++;
            break;
          }
        }

        // should not get past this point
      }
    }

    /// <summary>
    /// Processes an elevation sub grid into a cut fill isopach and calculate the counts of cells where the cut fill
    /// height fits into the requested bands
    /// </summary>
    public override void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      _lock.Wait();
      try
      {
        base.ProcessSubGridResult(subGrids);

        // Works out the percentage each colour on the map represents

        foreach (var subGrid in subGrids)
        {
          if ((subGrid?.Length ?? 0) > 0 && subGrid[0] is ClientHeightLeafSubGrid SubGrid)
          {
            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
              var Value = SubGrid.Cells[I, J];
              if (Value != Consts.NullHeight) // is there a value to test
              {
                SummaryCellsScanned++;
                IncrementCountOfCutFillTransition(Value);
              }
            });
          }
        }
      }
      finally
      {
        _lock.Release();
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _lock.Dispose();
          _lock = null;
        }

        base.Dispose(disposing);

        _disposedValue = true;
      }
    }
  }
}
