using System;
using System.Diagnostics;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees
{
  /// <summary>
    /// SubGridTreeBitMask implements a subgrid tree whose sole contents is a single
    /// bit per cell in the leaf nodes. The intention of this SubGridTree descendant
    /// is for tasks such as tracking cells modified since last read, or cells present
    /// in a SubGridTree cache etc. It maintains both per cell and per leaf flags.
    /// </summary>
  public class SubGridTreeBitMask : SubGridTree, ISubGridTreeBitMask
  {
        /// <summary>
        /// Constructor that defaults levels, cell size and subgrid factory 
        /// </summary>
        public SubGridTreeBitMask() : base(SubGridTreeConsts.SubGridTreeLevels,
                                           SubGridTreeConsts.DefaultCellSize, 
                                           new SubGridFactory<SubGridTreeNodeBitmapSubGrid, SubGridTreeLeafBitmapSubGrid>())
        {          
        }

        /// <summary>
        /// Constructor that sets levels and cell size with a default factory
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        public SubGridTreeBitMask(byte numLevels, double cellSize) : base(numLevels, cellSize,
                                  new SubGridFactory<SubGridTreeNodeBitmapSubGrid, SubGridTreeLeafBitmapSubGrid>())
        {
        }

        /// <summary>
        /// Constructor accepting number of levels, cell size and subgrid factory aspects.
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        /// <param name="subGridfactory"></param>
        public SubGridTreeBitMask(byte numLevels,
                                  double cellSize,
                                  ISubGridFactory subGridfactory) : base(numLevels, cellSize, subGridfactory)
        {
        }

        /// <summary>
        /// Performs the fundamental GetCell operation that returns a boolean value noting the state of the 
        /// bit in the tree at the [CellX, CellY] location
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>                        
        public bool GetCell(uint CellX, uint CellY)
        {
            ISubGrid SubGrid = LocateSubGridContaining(CellX, CellY, NumLevels);

            if (SubGrid == null)
                return false;

            SubGrid.GetSubGridCellIndex(CellX, CellY, out byte SubGridX, out byte SubGridY);

            return (SubGrid as SubGridTreeLeafBitmapSubGrid).Bits.BitSet(SubGridX, SubGridY);
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs the fundamental SetCell operation that sets the state of bit in the tree at the 
        /// [CellX, CellY] location according to the boolean value parameter
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="Value"></param>
        /// <returns></returns>     
        public void SetCell(uint CellX, uint CellY, bool Value)
        {
            ISubGrid SubGrid = ConstructPathToCell(CellX, CellY, SubGridPathConstructionType.CreateLeaf);

            if (SubGrid == null)
            {
                Debug.Assert(false, "Unable to create cell subgrid for bitmask");
            }

            SubGrid.GetSubGridCellIndex(CellX, CellY, out byte SubGridX, out byte SubGridY);
            (SubGrid as SubGridTreeLeafBitmapSubGrid).Bits.SetBitValue(SubGridX, SubGridY, Value);
        }

        /// <summary>
        /// Calculates the integer bounding rectangle within the bit mask subgrid that encloses all bits that
        /// are set to 1 (true)
        /// </summary>
        /// <returns></returns>
        private BoundingIntegerExtent2D ComputeCellsExtents()
        {
            BoundingIntegerExtent2D SubGridCellsExtents = new BoundingIntegerExtent2D();
            SubGridCellsExtents.SetInverted();

            ScanAllSubGrids(x => 
            {
                SubGridCellsExtents.Include((x as SubGridTreeLeafBitmapSubGrid).ComputeCellsExtents());
                return true;
            });

            return SubGridCellsExtents;
        }

        /// <summary>
        /// Default array indexer for the bits in the subgrid tree mask
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool this[uint CellX, uint CellY]
        {
            get => GetCell(CellX, CellY);
            set => SetCell(CellX, CellY, value);
        }

        /// <summary>
        /// RemoveLeafOwningCell locates the leaf subgrid that contains the OTG cell identified by CellX and CellY and removes it from the
        /// sub grid tree.        
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void RemoveLeafOwningCell(uint CellX, uint CellY)
        {
            ISubGrid SubGrid = LocateSubGridContaining(CellX, CellY, (byte)(NumLevels - 1));

            if (SubGrid == null)
                return;

            SubGrid.GetSubGridCellIndex(CellX, CellY, out byte SubGridX, out byte SubGridY);

            SubGridTreeNodeBitmapSubGrid bitmapSubGrid = SubGrid as SubGridTreeNodeBitmapSubGrid;

            // Free the node containing the bits for the cells in the leaf
            bitmapSubGrid.SetSubGrid(SubGridX, SubGridY, null);
        }

        /// <summary>
        /// CountBits performs a scan of the subgrid bit mask tree and counts all the bits that are set within it
        /// </summary>
        /// <returns></returns>
        public long CountBits()
        {
            long totalBits = 0;

            ScanAllSubGrids(x => 
            {
                totalBits += (x as SubGridTreeLeafBitmapSubGrid).Bits.CountBits();
                return true;
            });

            return totalBits;
        }

        /// <summary>
        /// Calculates the world coordinate bounding rectangle within the bit mask subgrid that encloses all bits that
        /// are set to 1 (true)
        /// </summary>
        /// <returns></returns>
        public BoundingWorldExtent3D ComputeCellsWorldExtents()
        {
            BoundingIntegerExtent2D SubGridCellsExtents = ComputeCellsExtents();

            if (SubGridCellsExtents.IsValidExtent)
            {
                GetCellCenterPosition((uint)SubGridCellsExtents.MinX, (uint)SubGridCellsExtents.MinY, out double MinCX, out double MinCY);
                GetCellCenterPosition((uint)SubGridCellsExtents.MaxX, (uint)SubGridCellsExtents.MaxY, out double MaxCX, out double MaxCY);

                return new BoundingWorldExtent3D(MinCX - CellSizeOver2, MinCY - CellSizeOver2,
                                                 MaxCX + CellSizeOver2, MaxCY + CellSizeOver2);
            }
            else
            {
                return BoundingWorldExtent3D.Inverted();
            }
        }

        /// <summary>
        /// LeafExists determines if there is a leaf cell in the sub grid tree that contains the cell at address [CellX, CellY].
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool LeafExists(uint CellX, uint CellY)
        {
            return ConstructPathToCell(CellX, CellY, SubGridPathConstructionType.ReturnExistingLeafOnly) != null;
        }

        /// <summary>
        /// Takes a source SubGridBitMask instance and performs a bitwise OR of the contents of source against the
        /// contents of this instance, modifying the state of this subgrid bit mask tree to produce the result
        /// </summary>
        /// <param name="Source"></param>
        public void SetOp_OR(ISubGridTreeBitMask Source)
        {
            SubGridTreeLeafBitmapSubGrid bitMapSubGrid;

            Source.ScanAllSubGrids(x =>
            {
                if (x != null)
                {
                    bitMapSubGrid = ConstructPathToCell(x.OriginX, x.OriginY, SubGridPathConstructionType.CreateLeaf) as SubGridTreeLeafBitmapSubGrid;
                    if (bitMapSubGrid != null)
                    {
                        // In this instance, x is a subgrid from the tree we are ORring with this
                        // one, and BitMapSubGrid is a grid retrieved from this tree
                        bitMapSubGrid.Bits.OrWith((x as SubGridTreeLeafBitmapSubGrid).Bits);
                    }
                    else
                    {
                        Debug.Assert(false, "Failed to create bit map subgrid in SetOp_OR");
                    }
                }

                return true; // Keep the scan going
            });
        }

        /// <summary>
        /// Takes a source SubGridBitMask instance and performs a bitwise AND of the contents of source against the
        /// contents of this instance, modifying the state of this subgrid bit mask tree to produce the result
        /// </summary>
        /// <param name="Source"></param>
        public void SetOp_AND(ISubGridTreeBitMask Source)
        {
            SubGridTreeLeafBitmapSubGrid bitMapSubGrid;

            /* Previous implementation iterated across the source, when only iteration across 'this' is required as
             * subgrids not present in this tree are implicitly 'false' so will never generate any true bits needing storing.
             * Similarly, subgrids in source that are not present in this will never generate any true bits requiring storing
            Source.ScanAllSubGrids(x =>
            {
                if (x == null)
                {
                    return true; // Keep the scan going
                }

                bitMapSubGrid = (SubGridTreeLeafBitmapSubGrid)(ConstructPathToCell(x.OriginX, x.OriginY, SubGridPathConstructionType.CreateLeaf));
                if (bitMapSubGrid != null)
                {
                    bitMapSubGrid.Bits = bitMapSubGrid.Bits & ((SubGridTreeLeafBitmapSubGrid)x).Bits;
                }
                else
                {
                    Debug.Assert(false, "Failed to create bit map subgrid in SetOp_AND");
                }

                return true; // Keep the scan going
            });
            */

            // This implementation will be much more performant!
            ScanAllSubGrids(x =>
            {
                if (x != null)
                {
                    bitMapSubGrid = Source.LocateSubGridContaining(x.OriginX, x.OriginY) as SubGridTreeLeafBitmapSubGrid;
                    if (bitMapSubGrid != null)
                        (x as SubGridTreeLeafBitmapSubGrid).Bits.AndWith(bitMapSubGrid.Bits);
                }

                return true; // Keep the scan going
            });
        }

        /// <summary>
        ///  ClearCellIfSet will set the value of a cell to false if the current
        /// value of cell is True. The function returns true if the cell was set
        /// and has been cleared
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool ClearCellIfSet(uint CellX, uint CellY)
        {
            ISubGrid SubGrid = LocateSubGridContaining(CellX, CellY, NumLevels);

            if (SubGrid == null)
                return false;

            SubGridTreeLeafBitmapSubGrid bitmapSubGrid = SubGrid as SubGridTreeLeafBitmapSubGrid;

            bitmapSubGrid.GetSubGridCellIndex(CellX, CellY, out byte SubGridX, out byte SubGridY);

            if (bitmapSubGrid.Bits.BitSet(SubGridX, SubGridY))
            {
                bitmapSubGrid.Bits.ClearBit(SubGridX, SubGridY);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Scan all the bits in the bit mask subgrid tree treating each set bit as the address of a subgrid
        /// call the supplied Action 'functor' with a leaf subgrid origin address calculated from each of the bits
        /// Note: As each bit represents an on-the-ground leaf subgrid, cell address of that bit needs to be transformed
        /// from the level 5 (node) layer to the level 6 (leaf) layer
        /// </summary>
        /// <param name="functor"></param>
        public void ScanAllSetBitsAsSubGridAddresses(Action<ISubGridCellAddress> functor)
        {
          ScanAllSubGrids(leaf =>
          {
            ((SubGridTreeLeafBitmapSubGrid)leaf).Bits.ForEachSetBit((x, y) =>
              functor(new SubGridCellAddress((uint)(leaf.OriginX + x) << SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                             (uint)(leaf.OriginY + y) << SubGridTreeConsts.SubGridIndexBitsPerLevel))
            );

            return true;
          });
        }

    public override string SerialisedHeaderName() => "ExistenceMap";

    public override int SerialisedVersion() => 1;
  }
}
