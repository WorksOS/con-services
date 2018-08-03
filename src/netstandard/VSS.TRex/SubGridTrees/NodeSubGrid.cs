using System;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// Base class for 'node' subgrids that form the structure of a subgridtree at all levels from the root
    /// to the level above the leaf subgrids at the bottom layer of the tree.
    /// </summary>
    public class NodeSubGrid : SubGrid, INodeSubGrid
    {
        /// <summary>
        /// The array of sparse cell refernces that form the known cells in this subgrid
        /// </summary>
        private SubgridTreeSparseCellRecord[] SparseCells;

        /// <summary>
        /// The number of sparse cells in the subgrid
        /// </summary>
        private short SparseCellCount;

        /// <summary>
        /// The non-sparse collection of child cell references
        /// </summary>
        private ISubGrid[,] Cells;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public NodeSubGrid()
        {
            SparseCellCount = 0;
            Cells = null;
        }

        /// <summary>
        /// Base constructor for a Node type subgrid.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public NodeSubGrid(ISubGridTree owner,
                           ISubGrid parent,
                           byte level) : base(owner, parent, level)
        {
            SparseCellCount = 0;
            Cells = null;
        }

        /// <summary>
        /// Clears the content from the node subgrid.
        /// </summary>
        public override void Clear()
        {
            ForEachSubGrid((i, j, subgrid) =>
            {
                SetSubGrid(i, j, null);
                return SubGridProcessNodeSubGridResult.OK;
            });

            Cells = null;
            SparseCells = null;
            SparseCellCount = 0;
        }

        /// <summary>
        /// DeleteSubgrod removes the subgrid present at the cell coordinates given
        /// by SubGridX and SubGridY within this subgrid. The removed subgrid is freed.
        /// This operation is by definition only relevant to node subgrids. Leaf
        /// subgrids do not contain child subgrids
        /// </summary>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        /// <param name="DeleteIfLocked"></param>
        public void DeleteSubgrid(byte SubGridX, byte SubGridY, bool DeleteIfLocked)
        {
            ISubGrid Subgrid = GetSubGrid(SubGridX, SubGridY);

            if (Subgrid != null)
            {
                SetSubGrid(SubGridX, SubGridY, null);

                //      if DeleteIfLocked and Subgrid.Locked then
                //        Subgrid.ReleaseLock(Subgrid.LockToken);
            }
        }

        /// <summary>
        /// Retrieves a child subgrid at the X, Y location from those that make up the subgrids within this subgrid.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public override ISubGrid GetSubGrid(byte X, byte Y)
        {
            if (Cells != null)
                return Cells[X, Y];

            if (SparseCells != null)
            {
                for (int I = 0; I < SparseCellCount; I++)
                {
                    SubgridTreeSparseCellRecord sparceCell = SparseCells[I];

                    if ((sparceCell.CellX == X) && (sparceCell.CellY == Y))
                        return sparceCell.Cell;
                }
            }

            return null;
        }

        /// <summary>
        /// GetSubGridContainingCell takes an on the ground cell coordinate and returns
        /// the subgrid X an Y address in this subgrid that contains it.
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
        /// IsEmpty determines if this node subgrid references any other subgrids lower in the tree
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            bool AnyNonNullItems = false;

            ForEachSubGrid(subgrid =>
            {
                AnyNonNullItems = true; // Found a non-null one, can stop looking now
                return SubGridProcessNodeSubGridResult.TerminateProcessing;
            });

            return !AnyNonNullItems;
        }

        /// <summary>
        /// Iterate over every child subgrid that is present within this subgrid. Each subgrid is presented to functor
        /// as single parameter (ISubGrid) reference to that subgrid. 
        /// Child subgrid references in this subgrid that are null are not presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <param name="minSubGridCellX"></param>
        /// <param name="minSubGridCellY"></param>
        /// <param name="maxSubGridCellX"></param>
        /// <param name="maxSubGridCellY"></param>
        public void ForEachSubGrid(Func<ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0,
            byte minSubGridCellY = 0,
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1,
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1)
        {
            if (minSubGridCellX >= SubGridTree.SubGridTreeDimension ||
                minSubGridCellY >= SubGridTree.SubGridTreeDimension)
            {
                throw new ArgumentException("Min/max subgrid cell X/Y bounds are out of range", 
                                             "minSubGridCellX, minSubGridCellY, maxnSubGridCellX, maxnSubGridCellY");
            }

            // Make use of the three parameter functor verion of ForEachSubgrid and ignore the subgrid location paramters.
            ForEachSubGrid((x, y, subgrid) => functor(subgrid),
                           minSubGridCellX, minSubGridCellY, maxSubGridCellX, maxSubGridCellY);
        }

        /// <summary>
        /// <param name="functor"></param>
        /// </summary>
        /// <param name="functor"></param>
        /// <param name="minSubGridCellX"></param>
        /// <param name="minSubGridCellY"></param>
        /// <param name="maxSubGridCellX"></param>
        /// <param name="maxSubGridCellY"></param>
        public void ForEachSubGrid(Func<byte, byte, ISubGrid, SubGridProcessNodeSubGridResult> functor,
            byte minSubGridCellX = 0, 
            byte minSubGridCellY = 0, 
            byte maxSubGridCellX = SubGridTree.SubGridTreeDimensionMinus1, 
            byte maxSubGridCellY = SubGridTree.SubGridTreeDimensionMinus1)
        {
            if (minSubGridCellX >= SubGridTree.SubGridTreeDimension ||
                minSubGridCellY >= SubGridTree.SubGridTreeDimension)
            {
                throw new ArgumentException("Min/max subgrid cell X/Y bounds are out of range",
                                             "minSubGridCellX, minSubGridCellY, maxnSubGridCellX, maxnSubGridCellY");
            }

            if (Cells != null)
            {
                for (byte I = minSubGridCellX; I <= maxSubGridCellX; I++)
                {
                    for (byte J = minSubGridCellY; J <= maxSubGridCellY; J++)
                    {
                        if ((Cells[I, J] != null) && (functor(I, J, Cells[I, J]) != SubGridProcessNodeSubGridResult.OK))
                            return;
                    }
                }

                return;
            }

            if (SparseCells != null)
            {
                for (int I = 0; I < SparseCellCount; I++)
                {
                    SubgridTreeSparseCellRecord sparceCell = SparseCells[I];

                    if ((sparceCell.CellX >= minSubGridCellX && sparceCell.CellX <= maxSubGridCellX &&
                         sparceCell.CellY >= minSubGridCellY && sparceCell.CellY <= maxSubGridCellY) &&
                       (functor(sparceCell.CellX, sparceCell.CellY, sparceCell.Cell) != SubGridProcessNodeSubGridResult.OK))
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// ScanSubGrids scans all subgrids at a requested level in the tree that
        /// intersect the given real world extent. Each subgrid that exists in the
        /// extent is passed to the OnProcessLeafSubgrid event for processing 
        /// leafFunctor and nodeFunctor are delegate/events called when scanning
        /// leaf subgrids in the sub grid tree (or any other events where the
        /// a leaf subgrid needs to passed to a processor). A return result of False 
        /// from a functor indicates the receiver of the event has requested the scanning process stop.
        /// </summary>
        /// <param name="Extent"></param>
        /// <param name="leafFunctor"></param>
        /// <param name="nodeFunctor"></param>
        /// <returns>A boolean indicating the ScanSubGrids operation was successful andnot aborted by a functor</returns>
        public bool ScanSubGrids(BoundingIntegerExtent2D Extent,
                                 Func<ISubGrid, bool> leafFunctor = null,
                                 Func<ISubGrid, SubGridProcessNodeSubGridResult> nodeFunctor = null)
        {
            // Allow the scanner to deal with the node subgrid and short circuit scanning here is desired
            if (nodeFunctor != null && nodeFunctor(this) == SubGridProcessNodeSubGridResult.TerminateProcessing)
                return false;

            // Work out the on-the-ground cell extent needed to be scanned that this sub grid covers
            uint ScanMinX = (uint)Math.Max(OriginX, Extent.MinX);
            uint ScanMinY = (uint)Math.Max(OriginY, Extent.MinY);
            uint ScanMaxX = (uint)Math.Min(OriginX + AxialCellCoverageByThisSubgrid() - 1, Extent.MaxX);
            uint ScanMaxY = (uint)Math.Min(OriginY + AxialCellCoverageByThisSubgrid() - 1, Extent.MaxY);

            // Convert the on-the-ground cell indexes into subgrid indexes at this level in the sub grid tree
            GetSubGridCellIndex(ScanMinX, ScanMinY, out byte SubGridMinX, out byte SubGridMinY);
            GetSubGridCellIndex(ScanMaxX, ScanMaxY, out byte SubGridMaxX, out byte SubGridMaxY);

            ForEachSubGrid(subgrid =>
            {
                if (leafFunctor != null && subgrid.IsLeafSubGrid()) // Leaf subgrids are passed to leafFunctor
                    return (leafFunctor(subgrid)) ? SubGridProcessNodeSubGridResult.OK : SubGridProcessNodeSubGridResult.TerminateProcessing;

                // Node subgrids are descended into recursively to continue processing
                return (!((INodeSubGrid)(subgrid)).ScanSubGrids(Extent, leafFunctor, nodeFunctor)) ? SubGridProcessNodeSubGridResult.TerminateProcessing : SubGridProcessNodeSubGridResult.OK;
            }, 
            SubGridMinX, SubGridMinY, SubGridMaxX, SubGridMaxY);

            return true;
        }

        /// <summary>
        /// Set a child subgrid at the X, Y location into the set of subgrids that are contained in this subgrid.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Value"></param>
        public override void SetSubGrid(byte X, byte Y, ISubGrid Value)
        {
            // Set the origin position and level for the subgrid as these quantities are
            // relative to the location of the subgrid in the tree. Throw an exception if the 
            // level of the sudgrid is not 0 (null), and is not the same as this.Level + 1
            // (ie: the caller is trying to be too clever!)
            if (Value != null)
            {
                if (Value.Level != 0 && Value.Level != Level + 1)
                    throw new ArgumentException("Level of subgrid being added is non-null and is not set correctly for the level it is being added to", "Value.Level");

                Value.Parent = this;
                Value.SetOriginPosition(X, Y);
                Value.Level = (byte)(Level + 1);
            }

            if (Cells != null)
            {
                Cells[X, Y] = Value;
                return;
            }

            if (Value != null)
            {
                if (SparseCells == null)
                {
                    SparseCells = new SubgridTreeSparseCellRecord[TRexConfig.SubGridTreeNodeCellSparcityLimit()];
                    SparseCellCount = 0;
                }

                // Add it to the sparse list
                if (SparseCellCount < TRexConfig.SubGridTreeNodeCellSparcityLimit())
                {
                    SparseCells[SparseCellCount++] = new SubgridTreeSparseCellRecord(X, Y, Value);
                }
                else
                {
                    // Create the full array of subgrid references now the number of subgrids is too large to 
                    // fit into the sparcity constraint
                    Cells = new ISubGrid[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

                    for (int I = 0; I < SparseCellCount; I++)
                    {
                        SubgridTreeSparseCellRecord sparceCell = SparseCells[I];
                        Cells[sparceCell.CellX, sparceCell.CellY] = sparceCell.Cell;
                    }

                    SparseCellCount = 0;
                    SparseCells = null;

                    // Add the new subgrid into the Cells array
                    Cells[X, Y] = Value;
                }
            }
            else
            {
                for (int I = 0; I < SparseCellCount; I++)
                {
                    if (SparseCells[I].CellX == X && SparseCells[I].CellY == Y)
                    {
                        if (I < SparseCellCount - 1)
                            Array.Copy(SparseCells, I + 1, SparseCells, I, SparseCellCount - I);

                        SparseCellCount--;
                        if (SparseCellCount == 0)
                            SparseCells = null;

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

            ForEachSubGrid(subgrid => 
            {
                count++;
                return SubGridProcessNodeSubGridResult.OK;
            });

            return count;
        }
    }
}
