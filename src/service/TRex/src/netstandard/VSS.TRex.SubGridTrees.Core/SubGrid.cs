using System;
using System.IO;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// The base class representing the concept of a sub grid within a sub grid tree
    /// </summary>
    public class SubGrid : ISubGrid
    {
        /// <summary>
        /// Create a human readable string representing the location and tree level this sub grid occupies in the tree.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Level:{Level}, OriginX:{OriginX}, OriginY:{OriginY}";

        /// <summary>
        /// The sub grid tree instance to which this sub grid belongs
        /// </summary>
        public ISubGridTree Owner { get; set; }

        /// <summary>
        /// The parent sub grid that owns this sub grid as a cell.
        /// </summary>
        public ISubGrid Parent { get; set; }

        /// <summary>
        /// ‘Level’ in the sub grid tree in which this sub grid resides. Level 1 is the root node in the tree, level 0 is invalid
        /// </summary>
        public byte Level { get; set; } // Invalid

        /// <summary>
        /// Grid cell X Origin of the bottom left hand cell in this sub grid. 
        /// Origin is wrt to cells of the spatial dimension held by this sub grid
        /// </summary>
        public int OriginX { get; set; } // int.MinValue;

        /// <summary>
        /// Grid cell Y Origin of the bottom left hand cell in this sub grid. 
        /// Origin is wrt to cells of the spatial dimension held by this sub grid
        /// </summary>
        public int OriginY { get; set; } // int.MinValue;

        /// <summary>
        /// Dirty property used to indicate the presence of changes that are not persisted.
        /// </summary>
        public bool Dirty { get; private set; }

        /// <summary>
        /// Sets the dirty flag state for the sub grid to true. See AllChangesMigrated for clearing this flag.
        /// </summary>
        public void SetDirty() => Dirty = true;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGrid()
        {
        }

        /// <summary>
        /// Basic constructor used to create base sub grid types that are not concerned with cell size
        /// or sub grid tree index origin offset aspects
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGrid(ISubGridTree owner, ISubGrid parent, byte level)
        {
            Owner = owner;
            Parent = parent;
            Level = level;
        }

        /// <summary>
        /// The number of on-the-ground cells that the span of this sub grid covers along each axis
        /// </summary>
        /// <returns></returns>
        public int AxialCellCoverageByThisSubGrid() => SubGridTreeConsts.SubGridTreeDimension << ((Owner.NumLevels - Level) * SubGridTreeConsts.SubGridIndexBitsPerLevel);

        /// <summary>
        /// The number of on-the-ground cells that the span of a child sub grid of this sub grid covers along each axis
        /// </summary>
        public int AxialCellCoverageByChildSubGrid() => AxialCellCoverageByThisSubGrid() >> SubGridTreeConsts.SubGridIndexBitsPerLevel;

        /// <summary>
        /// Sets the origin position of this sub grid to the supplied X and Y values within the cells of the parent sub grid. 
        /// This action locks the location of this sub grid in space with respect to the origin position of the parent sub grid.
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void SetOriginPosition(int CellX, int CellY)
        {
            if (Parent == null)
            { 
               throw new ArgumentException("Cannot set origin position without parent");
            }

            if (CellX >= SubGridTreeConsts.SubGridTreeDimension || CellY >= SubGridTreeConsts.SubGridTreeDimension)
            {
                throw new ArgumentException("Cell X, Y location is not in the valid cell address range for the sub grid");
            }

            OriginX = Parent.OriginX + CellX * AxialCellCoverageByThisSubGrid();
            OriginY = Parent.OriginY + CellY * AxialCellCoverageByThisSubGrid();
        }

        /// <summary>
        /// SetAbsoluteOriginPosition sets the origin position for this cell in terms
        /// of absolute cell origin coordinates.
        /// At the current time, it is only valid to do if the sub grid does not have a
        /// parent (in which case SetOriginPosition should be used);
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        public void SetAbsoluteOriginPosition(int originX, int originY)
        {
            if (Parent != null)
            {
                throw new Exception("Nodes referencing parent nodes may not have their origin modified");
            }

            OriginX = originX;
            OriginY = originY;
        }

        /// <summary>
        /// Determines the local in-sub grid X/Y location of a cell given its absolute cell index.
        /// This is a sub grid relative operation only, and depends only on the Owner to derive the difference
        /// between the number of levels in the overall tree, and the level in the tree at which this sub grid resides 
        /// to compute the sub grid relative X and y cell indices as it is a leaf sub grid.
        /// WARNING: This call assumes the cell index does lie within this sub grid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        public void GetSubGridCellIndex(int CellX, int CellY, out byte SubGridX, out byte SubGridY)
        {
            int SHRValue = (Owner.NumLevels - Level) * SubGridTreeConsts.SubGridIndexBitsPerLevel;
            SubGridX = (byte)((CellX >> SHRValue) & SubGridTreeConsts.SubGridLocalKeyMask);
            SubGridY = (byte)((CellY >> SHRValue) & SubGridTreeConsts.SubGridLocalKeyMask);
        }

        /// <summary>
        /// GetOTGLeafSubGridCellIndex determines the local in-sub grid X/Y location of a
        /// cell given its absolute cell index in an on-the-ground leaf sub grid where the level of the sub grid is implicitly known
        /// to be the same as Owner.NumLevels. Do not call this method for a sub grid that is not a leaf sub grid
        /// WARNING: This call assumes the cell index does lie within this sub grid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        public void GetOTGLeafSubGridCellIndex(int CellX, int CellY, out byte SubGridX, out byte SubGridY)
        {
            SubGridX = (byte)(CellX & SubGridTreeConsts.SubGridLocalKeyMask);
            SubGridY = (byte)(CellY & SubGridTreeConsts.SubGridLocalKeyMask);
        }

        /// <summary>
        /// Determine if this sub grid represents a leaf sub grid containing information for on-the-ground cells
        /// </summary>
        public bool IsLeafSubGrid() => Level == Owner.NumLevels;

        /// <summary>
        /// Returns a moniker string comprised of the X and Y origin ordinates in the sub grid cell address space
        /// separated by a colon, eg: in the form 1234:5678
        /// </summary>
        public string Moniker() => $"{OriginX}:{OriginY}";

        /// <summary>
        /// A virtual method representing an access mechanism to request a child sub grid at the X/Y location in this sub grid
        /// Note: By definition, leaf sub grids do not have child sub grids.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public virtual ISubGrid GetSubGrid(int X, int Y) => null; // Base class does not have child sub grids

        /// <summary>
        /// A virtual method representing an access mechanism to request a child sub grid at the X/Y location in this sub grid
        /// Note: By definition, leaf sub grids do not have child sub grids.
        /// Note: The X, Y location is relative to the elements in the sub grid (ie: 0..dimension(x/x)-1)
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual void SetSubGrid(int X, int Y, ISubGrid value)
        {
          // No location to set sub grid to in base class
        }

        /// <summary>
        /// Calculates the location in the world coordinate/ system of the bottom left hand corner of the 
        /// bottom left hand on-the-ground corner of the bottom left hand on-the-ground cell in the grid
        /// </summary>
        /// <param name="WorldOriginX"></param>
        /// <param name="WorldOriginY"></param>
        public virtual void CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY)
        {
            WorldOriginX = (OriginX - Owner.IndexOriginOffset) * Owner.CellSize;
            WorldOriginY = (OriginY - Owner.IndexOriginOffset) * Owner.CellSize;
        }

        /// <summary>
        /// Clear sets all the entries in the grid to be unassigned, or null
        /// </summary>
        public virtual void Clear()
        {
           // Nothing to clear in base class
        }

        /// <summary>
        /// AllChangesMigrated tells this sub grid that any changes that have been made to
        /// it (and which resulted in the dirty flag being set) have been migrated to
        /// another location. This essentially just sets the dirty flag to false, but
        /// encapsulates the semantics that any changes have been dealt with/preserved
        /// externally to this sub grid
        /// 
        /// </summary>
        public void AllChangesMigrated() => Dirty = false;

        /// <summary>
        /// IsEmpty determines if this sub grid contains any information. By default the base 
        /// implementation is never empty
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEmpty() => false;

        /// <summary>
        /// RemoveFromParent removes the reference to this sub grid from the parent node
        /// sub grid. It does not free the sub grid, just removes it from the tree.
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent == null)
                return;

            Parent.GetSubGridCellIndex(OriginX, OriginY, out byte SubGridX, out byte SubGridY);
            Parent.SetSubGrid(SubGridX, SubGridY, null);

            Parent = null;
        }

        /// <summary>
        /// Determines if this sub grid contains the cell identified by an on-the-ground CellX and CellY location
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool ContainsOTGCell(int CellX, int CellY)
        {
           int AxialCoverage = AxialCellCoverageByThisSubGrid();

           return (CellX >= OriginX) && (CellX < OriginX + AxialCoverage) && (CellY >= OriginY) && (CellY < OriginY + AxialCoverage);
        }

        /// <summary>
        /// CellHasValue indicates if the cell identified by CellX, CellY has a value (hence is not null)
        /// CellHasValue queries the leaf sub grid to determine if the cell at the
        /// given X/Y location within it has a value. CellX and CellY are in the
        /// 0..SubGridTreeDimension-1 coordinate space of the sub grid.
        /// WARNING: This is a comparatively expensive operation and so should not be used with abandon!
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public virtual bool CellHasValue(byte CellX, byte CellY) => false;

        /// <summary>
        /// Counts the number of cells that are non null in the sub grid using the base CellHasValue() interface
        /// </summary>
        /// <returns></returns>
        public virtual int CountNonNullCells()
        {
            int result = 0;

            for (int I = 0; I < SubGridTreeConsts.SubGridTreeCellsPerSubGrid; I++)
            {
                if (CellHasValue((byte)(I / SubGridTreeConsts.SubGridTreeDimension), (byte)(I % SubGridTreeConsts.SubGridTreeDimension)))
                {
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// SetAbsoluteLevel sets the level field in this node. This is only valid
        /// to do if the node does not have a parent (in which case it's level is
        /// implicitly knowable, and should have been explicitly set)
        /// </summary>
        /// <param name="level"></param>
        public void SetAbsoluteLevel(byte level)
        {
            if (Parent != null)
            {
                throw new TRexSubGridTreeException("Nodes referencing parent nodes may not have their level modified");
            }

            Level = level;
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public virtual void Write(BinaryWriter writer, byte [] buffer)
        {
            writer.Write(Level);
            writer.Write(OriginX);
            writer.Write(OriginY);
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        public virtual void Read(BinaryReader reader, byte [] buffer)
        {
            Level = reader.ReadByte();
            OriginX = reader.ReadInt32();
            OriginY = reader.ReadInt32();
        }

        /// <summary>
        /// Converts the sub grid origin cell location into a SubGridAddress identifying this sub grid
        /// </summary>
        /// <returns></returns>
        public SubGridCellAddress OriginAsCellAddress() => new SubGridCellAddress(OriginX, OriginY);

        public byte[] ToBytes() => ToBytes(new byte[10000]);

        public byte[] ToBytes(byte[] helperBuffer) => FromToBytes.ToBytes(Write, helperBuffer ?? new byte[10000]);

        public void FromBytes(byte[] bytes, byte[] helperBuffer = null) => FromToBytes.FromBytes(bytes, Read, helperBuffer ?? new byte[10000]);
     
        /// <summary>
        /// Iterates over all the cells in the sub grid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public void ForEach(Action<byte, byte> functor) => SubGridUtilities.SubGridDimensionalIterator((x, y) => functor((byte)x, (byte)y));
    
     
        /// <summary>
        /// Iterates over all the cells in the sub grid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static void ForEachStatic(Action<byte, byte> functor) => SubGridUtilities.SubGridDimensionalIterator((x, y) => functor((byte)x, (byte)y));
    }
}

