using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Models.Classes.Integrator;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
  /// <summary>
  /// a grouped sub grid integrator integrates the same sub grid from a group of site model trees and returns a single sub grid
  /// containing the grouped result.
  /// Note 1: Operation of this functionality is most efficient if the set of sub grid trees are sorted in time order.
  /// Note 2: THis functionality assumes all contents of all supplied sub grid trees are present in memory, and that all sub grids
  /// contain a single segment
  /// </summary>
  public class GroupedSubGridIntegrator : IGroupedSubGridIntegrator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GroupedSubGridIntegrator>();

    private const int DEFAULT_CELL_PASS_ARRAY_SIZE = 1000;
    private const int DEFAULT_CELL_PASS_ARRAY_SIZE_INCREMENT = 1000;

    private List<(IServerSubGridTree, DateTime, DateTime)> _trees;
    public List<(IServerSubGridTree, DateTime, DateTime)> Trees
    {
      get => _trees;
      set
      {
        _trees = value;
        _subGridGroup = new IServerLeafSubGrid[_trees.Count];
      }
    }

    private IServerLeafSubGrid[] _subGridGroup;
    private int _subGridGroupCount;

    private CellPass[] _totalCellPasses = new CellPass[DEFAULT_CELL_PASS_ARRAY_SIZE];
    private int _numTotalCellPasses;

    private IServerLeafSubGrid _resultSubGrid;

    public long NumOutOfOrderCellPassInsertions;


    public GroupedSubGridIntegrator()
    {
    }

    private void ProcessCell(int x, int y)
    {
      int totalCellPassCount = 0;

      for (int i = 0; i < _subGridGroupCount; i++)
        totalCellPassCount += _subGridGroup[i].Cells.PassesData[0].PassesData.PassCount(x, y);

      if (totalCellPassCount == 0)
        return;

      if (_totalCellPasses.Length < totalCellPassCount)
        _totalCellPasses = new CellPass[totalCellPassCount + DEFAULT_CELL_PASS_ARRAY_SIZE_INCREMENT];

      var lastTime = DateTime.MinValue;
      _numTotalCellPasses = 0;

      for (int i = 0; i < _subGridGroupCount; i++)
      {
        var cell = _subGridGroup[i].Cells.PassesData[0].PassesData.ExtractCellPasses(x, y);
        var passes = cell.Passes;

        for (int cpi = 0, limit = passes.Count; cpi < limit; cpi++)
        {
          var cellPass = passes.GetElement(cpi);

          if (_numTotalCellPasses == 0 || lastTime < cellPass.Time)
          {
            _totalCellPasses[_numTotalCellPasses++] = cellPass;
            lastTime = cellPass.Time;
          }
          else
          {
            // Somehow an out of order cell pass has been presented. Take the 'unhappy' path of figuring out where to insert it
            // Find the appropriate location in the overall cell passes. If it is an exact match then replace it, otherwise
            // insert it. As the location is most likely to be very near the end of the list use a dum reverse linear search...
            // This is horribly non-performing in general but is expected to be very rare given that the site models are sorted
            // in time before integration by this code

            var index = _numTotalCellPasses - 1;
            while (index > 0 && _totalCellPasses[index].Time > cellPass.Time)
              index--;

            // Cell pass at index has same time as cellPass.Time, then just replace it, otherwise insert it.
            if (!_totalCellPasses[index].Time.Equals(cellPass.Time))
            {
              Array.Copy(_totalCellPasses, index, _totalCellPasses, index + 1, _numTotalCellPasses - index);
              _numTotalCellPasses++;
            }

            _totalCellPasses[index] = cellPass;

            NumOutOfOrderCellPassInsertions++;
          }
        }
      }

      if (_numTotalCellPasses > 0)
      {
        _resultSubGrid.Cells.PassesData[0].PassesData.ReplacePasses(x, y, _totalCellPasses, _numTotalCellPasses);
        _resultSubGrid.Directory.GlobalLatestCells.PassDataExistenceMap[(byte) x, (byte) y] = true;
      }
    }

    public void IntegrateSubGridGroup(IServerLeafSubGrid resultSubGrid)
    {
      _resultSubGrid = resultSubGrid;

      // Iterate over all cells in the sub grid.
      // --> Count the number of cell passes in the overall collection
      // --> Create a cell pass array enough big enough to hold all of them
      // --> Aggregate all cell passes from the sub grid group together

      _subGridGroupCount = 0;

      // Locate references to the sub grid in all the supplied models that have a sub grid present at subGridCellAddress
      foreach (var tree in _trees)
      {
        var subGrid = tree.Item1.LocateSubGridContaining(_resultSubGrid.OriginX, _resultSubGrid.OriginY);

        if (subGrid != null)
        {
          _subGridGroup[_subGridGroupCount++] = (IServerLeafSubGrid) subGrid;
        }
      }

      if (_subGridGroupCount == 0)
      {
        Log.LogWarning($"No sub grids found in grouped site models at sub grid {_resultSubGrid.Moniker()}");
        return;
      }

      _resultSubGrid.Directory.CreateDefaultSegment();
      _resultSubGrid.AllocateLeafFullPassStacks();
      _resultSubGrid.AllocateLeafLatestPassGrid();
      _resultSubGrid.AllocateSegment(resultSubGrid.Directory.SegmentDirectory[0]);
      _resultSubGrid.Cells.PassesData[0].AllocateFullPassStacks();

      SubGridUtilities.SubGridDimensionalIterator((Action<int, int>) ProcessCell);

      if (_resultSubGrid.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits() == 0)
      {
        Log.LogWarning($"No cells established in sub grids for in grouped site models at sub grid {_resultSubGrid.Moniker()}");
      }
    }
  }
}
