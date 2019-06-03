using System;
using System.Collections.Generic;
using VSS.TRex.Cells;
using VSS.TRex.Common.Exceptions;
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
    private const int Default_CELL_PASS_ARRAY_SIZE = 1000;
    private const int Default_CELL_PASS_ARRAY_SIZE_INCREMENT = 1000;

    public List<(IServerSubGridTree, DateTime, DateTime)> Trees { get; set; }

    private readonly List<IServerLeafSubGrid> _subGridGroup = new List<IServerLeafSubGrid>();
    private CellPass[] _cellPasses = new CellPass[Default_CELL_PASS_ARRAY_SIZE];
    private int _numCellPasses;

    public GroupedSubGridIntegrator()
    {
    }

    public void IntegrateSubGridGroup(IServerLeafSubGrid resultSubGrid) 
    {
      var subGridCount = 0;

      // Locate references to the sub grid in all the supplied models that have a sub grid present at subGridCellAddress
      foreach (var tree in Trees)
      {
        var subGrid = tree.Item1.LocateSubGridContaining(resultSubGrid.OriginX, resultSubGrid.OriginY);

        if (subGrid != null)
        {
          if (subGridCount < _subGridGroup.Count)
            _subGridGroup[subGridCount] = (IServerLeafSubGrid) subGrid;
          else
            _subGridGroup.Add((IServerLeafSubGrid) subGrid);
          subGridCount++;
        }
      }

      // Iterate over all cells in the sub grid.
      // --> Count the number of cell passes in the overall collection
      // --> Create a cell pass array enough big enough to hold all of them
      // --> Aggregate all cell passes from the sub grid group together

      resultSubGrid.Directory.CreateDefaultSegment();
      resultSubGrid.AllocateLeafFullPassStacks();
      resultSubGrid.AllocateSegment(resultSubGrid.Directory.SegmentDirectory[0]);
      resultSubGrid.Cells.PassesData[0].AllocateFullPassStacks();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        uint totalCellPassCount = 0;

        foreach (var subGrid in _subGridGroup)
          totalCellPassCount += subGrid.Cells.PassesData[0].PassesData.PassCount(x, y);

        if (_cellPasses.Length < totalCellPassCount)
          _cellPasses = new CellPass[totalCellPassCount + Default_CELL_PASS_ARRAY_SIZE_INCREMENT];

        foreach (var subGrid in _subGridGroup)
        {
          _numCellPasses = 0;
          var cellPasses = subGrid.Cells.PassesData[0].PassesData.ExtractCellPasses(x, y);

          if (cellPasses == null)
          {
            // There are no cell passes present in this cell in this sub grid
            continue;
          }

          foreach (var cellPass in cellPasses)
          {
            if (_numCellPasses == 0)
            {
              _cellPasses[_numCellPasses] = cellPass;
              continue;
            }

            if (_cellPasses[_numCellPasses - 1].Time < cellPass.Time)
            {
              _cellPasses[_numCellPasses] = cellPass;
            }
            else
            {
              // Enforce ordering criteria???
              // ...
              throw new TRexException("Site models not time ordered");
            }

            _numCellPasses++;
          }

          var resultCellPasses = new CellPass[_numCellPasses];
          Array.Copy(_cellPasses, resultCellPasses, _numCellPasses);

          resultSubGrid.Cells.PassesData[0].PassesData.ReplacePasses(x, y, resultCellPasses);
        }
      });
    }
  }
}
