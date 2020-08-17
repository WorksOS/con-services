using System;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Core
{
  /// <summary>
  /// Base class for 'node' sub grids that form the structure of a sub grid tree at all levels from the root
  /// to the level above the leaf sub grids at the bottom layer of the tree.
  /// </summary>
  public class NodeSubGrid : SubGrid, INodeSubGrid
  {
    /// <summary>
    /// The array of sparse cell references that form the known cells in this sub grid
    /// </summary>
    private SubGridTreeSparseCellRecord[] _sparseCells;

    /// <summary>
    /// The number of sparse cells in the sub grid
    /// </summary>
    private short _sparseCellCount;

    /// <summary>
    /// The non-sparse collection of child cell references
    /// </summary>
    private ISubGrid[,] _cells;

    /// <summary>
    /// The limit under which node sub grids are represented by sparse lists rather than a complete sub grid array of child sub grid references
    /// </summary>
    public static int SubGridTreeNodeCellSparcityLimit { get; } = DIContext.Obtain<IConfigurationStore>()?.GetValueInt("SUBGRIDTREENODE_CELLSPARCITYLIMIT", Consts.SUBGRIDTREENODE_CELLSPARCITYLIMIT) ?? Consts.SUBGRIDTREENODE_CELLSPARCITYLIMIT;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public NodeSubGrid()
    {
      Initialise();
    }

    /// <summary>
    /// Base constructor for a Node type sub grid.
    /// </summary>
    public NodeSubGrid(ISubGridTree owner,
      ISubGrid parent,
      byte level) : base(owner, parent, level)
    {
      Initialise();
    }

    private void Initialise()
    {
      _sparseCells = new SubGridTreeSparseCellRecord[SubGridTreeNodeCellSparcityLimit];
      _sparseCellCount = 0;
      _cells = null;
    }

    /// <summary>
    /// Clears the content from the node sub grid.
    /// </summary>
    public override void Clear()
    {
      ForEachSubGrid((i, j, subGrid) =>
      {
        SetSubGrid(i, j, null);
        return SubGridProcessNodeSubGridResult.OK;
      });

      _cells = null;
      _sparseCells = new SubGridTreeSparseCellRecord[SubGridTreeNodeCellSparcityLimit];
      _sparseCellCount = 0;
    }

    /// <summary>
    /// DeleteSubGrid removes the sub grid present at the cell coordinates given
    /// by SubGridX and SubGridY within this sub grid. The removed sub grid is freed.
    /// This operation is by definition only relevant to node sub grids. Leaf
    /// sub grids do not contain child sub grids
    /// </summary>
    public void DeleteSubGrid(int subGridX, int subGridY)
    {
      var subGrid = GetSubGrid(subGridX, subGridY);

      if (subGrid != null)
      {
        SetSubGrid(subGridX, subGridY, null);
      }
    }

    /// <summary>
    /// Retrieves a child sub grid at the X, Y location from those that make up the sub grids within this sub grid.
    /// </summary>
    public override ISubGrid GetSubGrid(int x, int y)
    {
      if (_cells != null)
        return _cells[x, y];

      for (var I = 0; I < _sparseCellCount; I++)
      {
        var sparseCell = _sparseCells[I];

        if (sparseCell.CellX == x && sparseCell.CellY == y)
          return sparseCell.Cell;
      }

      return null;
    }

    /// <summary>
    /// GetSubGridContainingCell takes an on the ground cell coordinate and returns
    /// the sub grid X an Y address in this sub grid that contains it.
    /// </summary>
    public virtual ISubGrid GetSubGridContainingCell(int cellX, int cellY)
    {
      GetSubGridCellIndex(cellX, cellY, out var subGridX, out var subGridY);

      return GetSubGrid(subGridX, subGridY);
    }

    /// <summary>
    /// IsEmpty determines if this node sub grid references any other sub grids lower in the tree
    /// </summary>
    public override bool IsEmpty()
    {
      var anyNonNullItems = false;

      ForEachSubGrid(subGrid =>
      {
        anyNonNullItems = true; // Found a non-null one, can stop looking now
        return SubGridProcessNodeSubGridResult.TerminateProcessing;
      });

      return !anyNonNullItems;
    }

    /// <summary>
    /// Iterate over every child sub grid that is present within this sub grid. Each sub grid is presented to functor
    /// as single parameter (ISubGrid) reference to that sub grid. 
    /// Child sub grid references in this sub grid that are null are not presented to functor.
    /// </summary>
    public void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor)
    {
      ForEachSubGrid(functor, 0, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1);
    }

    /// <summary>
    /// Iterate over every child sub grid that is present within this sub grid. Each sub grid is presented to functor
    /// as single parameter (ISubGrid) reference to that sub grid. 
    /// Child sub grid references in this sub grid that are null are not presented to functor.
    /// </summary>
    public void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY)
    {
      if (minSubGridCellX >= SubGridTreeConsts.SubGridTreeDimension ||
          minSubGridCellY >= SubGridTreeConsts.SubGridTreeDimension)
      {
        throw new ArgumentException("Minimum sub grid cell X/Y bounds are out of range");
      }

      // Make use of the three parameter functor version of ForEachSubGrid and ignore the sub grid location parameters.
      ForEachSubGrid((x, y, subGrid) => functor(subGrid),
        minSubGridCellX, minSubGridCellY, maxSubGridCellX, maxSubGridCellY);
    }

    /// <summary>
    /// Iterate over every child sub grid that is present within this sub grid. Each sub grid is presented to functor
    /// as single parameter (ISubGrid) reference to that sub grid. 
    /// Child sub grid references in this sub grid that are null are not presented to functor.
    /// </summary>
    public void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor)
    {
      ForEachSubGrid(functor, 0, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1);
    }

    /// <summary>
    /// Iterates over each sub grid contained in this node bounded by the min/max indices calling the supplied functor
    /// </summary>
    public void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY)
    {
      if (minSubGridCellX >= SubGridTreeConsts.SubGridTreeDimension ||
          minSubGridCellY >= SubGridTreeConsts.SubGridTreeDimension)
      {
        throw new ArgumentException("Minimum sub grid cell X/Y bounds are out of range");
      }

      if (_cells != null)
      {
        for (var i = minSubGridCellX; i <= maxSubGridCellX; i++)
        {
          for (var j = minSubGridCellY; j <= maxSubGridCellY; j++)
          {
            if ((_cells[i, j] != null) && (functor(i, j, _cells[i, j]) != SubGridProcessNodeSubGridResult.OK))
              return;
          }
        }

        return;
      }
      
      for (var I = 0; I < _sparseCellCount; I++)
      {
        var sparseCell = _sparseCells[I];

        if ((sparseCell.CellX >= minSubGridCellX && sparseCell.CellX <= maxSubGridCellX &&
             sparseCell.CellY >= minSubGridCellY && sparseCell.CellY <= maxSubGridCellY) &&
            (functor(sparseCell.CellX, sparseCell.CellY, sparseCell.Cell) != SubGridProcessNodeSubGridResult.OK))
        {
          return;
        }
      }
    }

    /// <summary>
    /// ScanSubGrids scans all sub grids at a requested level in the tree that
    /// intersect the given real world extent. Each sub grid that exists in the
    /// extent is passed to the OnProcessLeafSubGrid event for processing 
    /// leafFunctor and nodeFunctor are delegate/events called when scanning
    /// leaf sub grids in the sub grid tree (or any other events where the
    /// a leaf sub grid needs to passed to a processor). A return result of False 
    /// from a functor indicates the receiver of the event has requested the scanning process stop.
    /// </summary>
    /// <returns>A boolean indicating the ScanSubGrids operation was successful and not aborted by a functor</returns>
    public bool ScanSubGrids(BoundingIntegerExtent2D extent,
      Func<ISubGrid, bool> leafFunctor = null,
      Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
    {
      // Allow the scanner to deal with the node sub grid and short circuit scanning here if desired
      if (nodeFunctor != null && nodeFunctor(this) == SubGridProcessNodeSubGridResult.TerminateProcessing)
        return false;

      // Work out the on-the-ground cell extent needed to be scanned that this sub grid covers
      var scanMinX = Math.Max(originX, extent.MinX);
      var scanMinY = Math.Max(originY, extent.MinY);
      var scanMaxX = Math.Min(originX + AxialCellCoverageByThisSubGrid() - 1, extent.MaxX);
      var scanMaxY = Math.Min(originY + AxialCellCoverageByThisSubGrid() - 1, extent.MaxY);

      // Convert the on-the-ground cell indexes into sub grid indexes at this level in the sub grid tree
      GetSubGridCellIndex(scanMinX, scanMinY, out var subGridMinX, out var subGridMinY);
      GetSubGridCellIndex(scanMaxX, scanMaxY, out var subGridMaxX, subGridY: out var subGridMaxY);

      ForEachSubGrid(subGrid =>
        {
          if (leafFunctor != null && subGrid.IsLeafSubGrid()) // Leaf sub grids are passed to leafFunctor
            return leafFunctor(subGrid) ? SubGridProcessNodeSubGridResult.OK : SubGridProcessNodeSubGridResult.TerminateProcessing;

          // Node sub grids are descended into recursively to continue processing
          return !((INodeSubGrid) subGrid).ScanSubGrids(extent, leafFunctor, nodeFunctor) ? SubGridProcessNodeSubGridResult.TerminateProcessing : SubGridProcessNodeSubGridResult.OK;
        },
        subGridMinX, subGridMinY, subGridMaxX, subGridMaxY);

      return true;
    }

    /// <summary>
    /// Set a child sub grid at the X, Y location into the set of sub grids that are contained in this sub grid.
    /// </summary>
    public override void SetSubGrid(int x, int y, ISubGrid value)
    {
      // Set the origin position and level for the sub grid as these quantities are
      // relative to the location of the sub grid in the tree. Throw an exception if the 
      // level of the sub grid is not 0 (null), and is not the same as this.Level + 1
      // (ie: the caller is trying to be too clever!)
      if (value != null)
      {
        if (value.Level != 0 && value.Level != level + 1)
          throw new ArgumentException("Level of sub grid being added is non-null and is not set correctly for the level it is being added to", nameof(value.Level));

        value.Parent = this;
        value.SetOriginPosition(x, y);
        value.Level = (byte) (level + 1);
      }

      if (_cells != null)
      {
        _cells[x, y] = value;
        return;
      }

      if (value != null)
      {

        // Add it to the sparse list
        if (_sparseCellCount < SubGridTreeNodeCellSparcityLimit)
        {
          _sparseCells[_sparseCellCount++] = new SubGridTreeSparseCellRecord((byte) x, (byte) y, value);
        }
        else
        {
          // Create the full array of sub grid references now the number of sub grids is too large to 
          // fit into the sparcity constraint
          _cells = new ISubGrid[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

          for (var I = 0; I < _sparseCellCount; I++)
          {
            var sparseCell = _sparseCells[I];
            _cells[sparseCell.CellX, sparseCell.CellY] = sparseCell.Cell;
          }

          _sparseCellCount = 0;
          _sparseCells = null; // Release the sparse cells array

          // Add the new sub grid into the Cells array
          _cells[x, y] = value;
        }
      }
      else
      {
        for (var I = 0; I < _sparseCellCount; I++)
        {
          if (_sparseCells[I].CellX == x && _sparseCells[I].CellY == y)
          {
            if (I < _sparseCellCount - 1)
              Array.Copy(_sparseCells, I + 1, _sparseCells, I, _sparseCellCount - I - 1);

            _sparseCellCount--;

            // Clear the spare cell entry
            _sparseCells[_sparseCellCount] = new SubGridTreeSparseCellRecord();

            break;
          }
        }
      }
    }

    /// <summary>
    /// CountChildren returns a count of the non-null child cells in this node
    /// </summary>
    public int CountChildren()
    {
      var count = 0;

      ForEachSubGrid(subGrid =>
      {
        count++;
        return SubGridProcessNodeSubGridResult.OK;
      });

      return count;
    }

    public override bool CellHasValue(byte cellX, byte cellY) => GetSubGrid(cellX, cellY) != null;

    /// <summary>
    /// Calculate the memory used by this node subgrid. Assume the sub grid reference is the size of a long (8 bytes)
    /// </summary>
    public int SizeOf()
    {
       var sum = 0;
      if (_cells != null)
        sum += SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.SubGridTreeDimension * sizeof(long);
      if (_sparseCells != null)
        sum += _sparseCells.Length * SubGridTreeSparseCellRecord.SizeOf();

      sum += sizeof(short); // For sparse cell count

      return sum;
    }
  }
}
