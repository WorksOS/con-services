using System;
using VSS.ConfigurationStore;
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
    /// <returns></returns>
    private readonly int _subGridTreeNodeCellSparcityLimit = DIContext.Obtain<IConfigurationStore>()?.GetValueInt("SUBGRIDTREENODE_CELLSPARCITYLIMIT", Consts.SUBGRIDTREENODE_CELLSPARCITYLIMIT) ?? Consts.SUBGRIDTREENODE_CELLSPARCITYLIMIT;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public NodeSubGrid()
    {
      _sparseCellCount = 0;
      _cells = null;
    }

    /// <summary>
    /// Base constructor for a Node type sub grid.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    public NodeSubGrid(ISubGridTree owner,
      ISubGrid parent,
      byte level) : base(owner, parent, level)
    {
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
      _sparseCells = null;
      _sparseCellCount = 0;
    }

    /// <summary>
    /// DeleteSubGrid removes the sub grid present at the cell coordinates given
    /// by SubGridX and SubGridY within this sub grid. The removed sub grid is freed.
    /// This operation is by definition only relevant to node sub grids. Leaf
    /// sub grids do not contain child sub grids
    /// </summary>
    /// <param name="SubGridX"></param>
    /// <param name="SubGridY"></param>
    public void DeleteSubGrid(int SubGridX, int SubGridY)
    {
      ISubGrid SubGrid = GetSubGrid(SubGridX, SubGridY);

      if (SubGrid != null)
      {
        SetSubGrid(SubGridX, SubGridY, null);
      }
    }

    /// <summary>
    /// Retrieves a child sub grid at the X, Y location from those that make up the sub grids within this sub grid.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <returns></returns>
    public override ISubGrid GetSubGrid(int X, int Y)
    {
      if (_cells != null)
        return _cells[X, Y];

      if (_sparseCells != null)
      {
        for (int I = 0; I < _sparseCellCount; I++)
        {
          SubGridTreeSparseCellRecord sparseCell = _sparseCells[I];

          if ((sparseCell.CellX == X) && (sparseCell.CellY == Y))
            return sparseCell.Cell;
        }
      }

      return null;
    }

    /// <summary>
    /// GetSubGridContainingCell takes an on the ground cell coordinate and returns
    /// the sub grid X an Y address in this sub grid that contains it.
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <param name="SubGridX"></param>
    /// <param name="SubGridY"></param>
    /// <returns></returns>
    public bool GetSubGridContainingCell(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY)
    {
      GetSubGridCellIndex(CellX, CellY, out SubGridX, out SubGridY);

      return GetSubGrid(SubGridX, SubGridY) != null;
    }

    /// <summary>
    /// IsEmpty determines if this node sub grid references any other sub grids lower in the tree
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty()
    {
      bool AnyNonNullItems = false;

      ForEachSubGrid(subGrid =>
      {
        AnyNonNullItems = true; // Found a non-null one, can stop looking now
        return SubGridProcessNodeSubGridResult.TerminateProcessing;
      });

      return !AnyNonNullItems;
    }

    /// <summary>
    /// Iterate over every child sub grid that is present within this sub grid. Each sub grid is presented to functor
    /// as single parameter (ISubGrid) reference to that sub grid. 
    /// Child sub grid references in this sub grid that are null are not presented to functor.
    /// </summary>
    /// <param name="functor"></param>
    public void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor)
    {
      ForEachSubGrid(functor, 0, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1);
    }

    /// <summary>
    /// Iterate over every child sub grid that is present within this sub grid. Each sub grid is presented to functor
    /// as single parameter (ISubGrid) reference to that sub grid. 
    /// Child sub grid references in this sub grid that are null are not presented to functor.
    /// </summary>
    /// <param name="functor"></param>
    /// <param name="minSubGridCellX"></param>
    /// <param name="minSubGridCellY"></param>
    /// <param name="maxSubGridCellX"></param>
    /// <param name="maxSubGridCellY"></param>
    public void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY)
    {
      if (minSubGridCellX >= SubGridTreeConsts.SubGridTreeDimension ||
          minSubGridCellY >= SubGridTreeConsts.SubGridTreeDimension)
      {
        throw new ArgumentException("Min/max sub grid cell X/Y bounds are out of range");
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
    /// <param name="functor"></param>
    public void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor)
    {
      ForEachSubGrid(functor, 0, 0, SubGridTreeConsts.SubGridTreeDimensionMinus1, SubGridTreeConsts.SubGridTreeDimensionMinus1);
    }

    /// <summary>
    /// </summary>
    /// <param name="functor"></param>
    /// <param name="minSubGridCellX"></param>
    /// <param name="minSubGridCellY"></param>
    /// <param name="maxSubGridCellX"></param>
    /// <param name="maxSubGridCellY"></param>
    public void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
      byte minSubGridCellX,
      byte minSubGridCellY,
      byte maxSubGridCellX,
      byte maxSubGridCellY)
    {
      if (minSubGridCellX >= SubGridTreeConsts.SubGridTreeDimension ||
          minSubGridCellY >= SubGridTreeConsts.SubGridTreeDimension)
      {
        throw new ArgumentException("Min/max sub grid cell X/Y bounds are out of range");
      }

      if (_cells != null)
      {
        for (byte I = minSubGridCellX; I <= maxSubGridCellX; I++)
        {
          for (byte J = minSubGridCellY; J <= maxSubGridCellY; J++)
          {
            if ((_cells[I, J] != null) && (functor(I, J, _cells[I, J]) != SubGridProcessNodeSubGridResult.OK))
              return;
          }
        }

        return;
      }

      if (_sparseCells != null)
      {
        for (int I = 0; I < _sparseCellCount; I++)
        {
          SubGridTreeSparseCellRecord sparseCell = _sparseCells[I];

          if ((sparseCell.CellX >= minSubGridCellX && sparseCell.CellX <= maxSubGridCellX &&
               sparseCell.CellY >= minSubGridCellY && sparseCell.CellY <= maxSubGridCellY) &&
              (functor(sparseCell.CellX, sparseCell.CellY, sparseCell.Cell) != SubGridProcessNodeSubGridResult.OK))
          {
            return;
          }
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
    /// <param name="Extent"></param>
    /// <param name="leafFunctor"></param>
    /// <param name="nodeFunctor"></param>
    /// <returns>A boolean indicating the ScanSubGrids operation was successful and not aborted by a functor</returns>
    public bool ScanSubGrids(BoundingIntegerExtent2D Extent,
      Func<ISubGrid, bool> leafFunctor = null,
      Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
    {
      // Allow the scanner to deal with the node sub grid and short circuit scanning here is desired
      if (nodeFunctor != null && nodeFunctor(this) == SubGridProcessNodeSubGridResult.TerminateProcessing)
        return false;

      // Work out the on-the-ground cell extent needed to be scanned that this sub grid covers
      uint ScanMinX = (uint) Math.Max(OriginX, Extent.MinX);
      uint ScanMinY = (uint) Math.Max(OriginY, Extent.MinY);
      uint ScanMaxX = (uint) Math.Min(OriginX + AxialCellCoverageByThisSubGrid() - 1, Extent.MaxX);
      uint ScanMaxY = (uint) Math.Min(OriginY + AxialCellCoverageByThisSubGrid() - 1, Extent.MaxY);

      // Convert the on-the-ground cell indexes into sub grid indexes at this level in the sub grid tree
      GetSubGridCellIndex(ScanMinX, ScanMinY, out byte SubGridMinX, out byte SubGridMinY);
      GetSubGridCellIndex(ScanMaxX, ScanMaxY, out byte SubGridMaxX, out byte SubGridMaxY);

      ForEachSubGrid(subGrid =>
        {
          if (leafFunctor != null && subGrid.IsLeafSubGrid()) // Leaf sub grids are passed to leafFunctor
            return (leafFunctor(subGrid)) ? SubGridProcessNodeSubGridResult.OK : SubGridProcessNodeSubGridResult.TerminateProcessing;

          // Node sub grids are descended into recursively to continue processing
          return (!((INodeSubGrid) (subGrid)).ScanSubGrids(Extent, leafFunctor, nodeFunctor)) ? SubGridProcessNodeSubGridResult.TerminateProcessing : SubGridProcessNodeSubGridResult.OK;
        },
        SubGridMinX, SubGridMinY, SubGridMaxX, SubGridMaxY);

      return true;
    }

    /// <summary>
    /// Set a child sub grid at the X, Y location into the set of sub grids that are contained in this sub grid.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Value"></param>
    public override void SetSubGrid(int X, int Y, ISubGrid Value)
    {
      // Set the origin position and level for the sub grid as these quantities are
      // relative to the location of the sub grid in the tree. Throw an exception if the 
      // level of the sub grid is not 0 (null), and is not the same as this.Level + 1
      // (ie: the caller is trying to be too clever!)
      if (Value != null)
      {
        if (Value.Level != 0 && Value.Level != Level + 1)
          throw new ArgumentException("Level of sub grid being added is non-null and is not set correctly for the level it is being added to", nameof(Value.Level));

        Value.Parent = this;
        Value.SetOriginPosition((uint) X, (uint) Y);
        Value.Level = (byte) (Level + 1);
      }

      if (_cells != null)
      {
        _cells[X, Y] = Value;
        return;
      }

      if (Value != null)
      {
        if (_sparseCells == null)
        {
          _sparseCells = new SubGridTreeSparseCellRecord[_subGridTreeNodeCellSparcityLimit];
          _sparseCellCount = 0;
        }

        // Add it to the sparse list
        if (_sparseCellCount < _subGridTreeNodeCellSparcityLimit)
        {
          _sparseCells[_sparseCellCount++] = new SubGridTreeSparseCellRecord((byte) X, (byte) Y, Value);
        }
        else
        {
          // Create the full array of sub grid references now the number of sub grids is too large to 
          // fit into the sparcity constraint
          _cells = new ISubGrid[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

          for (int I = 0; I < _sparseCellCount; I++)
          {
            SubGridTreeSparseCellRecord sparseCell = _sparseCells[I];
            _cells[sparseCell.CellX, sparseCell.CellY] = sparseCell.Cell;
          }

          _sparseCellCount = 0;
          _sparseCells = null;

          // Add the new sub grid into the Cells array
          _cells[X, Y] = Value;
        }
      }
      else
      {
        for (int I = 0; I < _sparseCellCount; I++)
        {
          if (_sparseCells[I].CellX == X && _sparseCells[I].CellY == Y)
          {
            if (I < _sparseCellCount - 1)
              Array.Copy(_sparseCells, I + 1, _sparseCells, I, _sparseCellCount - I);

            _sparseCellCount--;
            if (_sparseCellCount == 0)
              _sparseCells = null;

            break;
          }
        }
      }
    }

    /// <summary>
    /// CountChildren returns a count of the non-null child cells in this node
    /// </summary>
    /// <returns></returns>
    public int CountChildren()
    {
      int count = 0;

      ForEachSubGrid(subGrid =>
      {
        count++;
        return SubGridProcessNodeSubGridResult.OK;
      });

      return count;
    }
  }
}
