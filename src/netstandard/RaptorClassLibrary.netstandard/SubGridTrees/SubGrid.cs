using System;
using System.IO;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// The base class representing the concept of a subgrid within a subgrid throw
    /// </summary>
    [Serializable]
    public class SubGrid : ISubGrid
    {
        /// <summary>
        /// Create a human readable string representing the location and tree level this subgrid occupies in the tree.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Level:{Level}, OriginX:{OriginX}, OriginY:{OriginY}";

        /// <summary>
        /// The sub grid tree instance to which this subgrid belongs
        /// </summary>
        public ISubGridTree Owner { get; set; }

        /// <summary>
        /// The parent subgrid that owns this subgrid as a cell.
        /// </summary>
        public ISubGrid Parent { get; set; }

        public bool Locked { get; set; }
        public int LockToken { get; set; } = -1;

        public bool AcquireLock(int lockToken)
        {
            lock (this)
            {
                if (Locked)
                    return false;

                Locked = true;
                LockToken = lockToken;

                return true;
            }
        }

        public void ReleaseLock(int lockToken)
        {
            LockToken = -1;
        }

        /// <summary>
        /// ‘Level’ in the subgridtree in which this subgrid resides. Level 1 is the root node in the tree, level 0 is invalid
        /// </summary>
        public byte Level { get; set; } // Invalid

        /// <summary>
        /// Grid cell X Origin of the bottom left hand cell in this subgrid. 
        /// Origin is wrt to cells of the spatial dimension held by this subgrid
        /// </summary>
        public uint OriginX { get; set; } // int.MinValue;

        /// <summary>
        /// Grid cell Y Origin of the bottom left hand cell in this subgrid. 
        /// Origin is wrt to cells of the spatial dimension held by this subgrid
        /// </summary>
        public uint OriginY { get; set; } // int.MinValue;

        /// <summary>
        /// Private backign store for the Dirty property. Descendent classes can override the GetDirty/SetDirty virtual methods
        /// to additional semantics to setting the dirty flag if required
        /// </summary>
        protected bool dirty;

        /// <summary>
        /// Dirty property used to indicate the presence of changes that are not persisted.
        /// </summary>
        public bool Dirty
        {
            get { return GetDirty(); }
            set { SetDirty(value); }
        }

        /// <summary>
        /// Default abstract implementation of GetDirty from the ISubGrid interface
        /// </summary>
        /// <returns></returns>
        public virtual bool GetDirty() => dirty;

        /// <summary>
        /// Default abstract implementation of SetDirty from the ISubGrid interface
        /// </summary>
        public virtual void SetDirty(bool value) => dirty = value;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGrid()
        {
        }

        /// <summary>
        /// Basic constructor used to create base subgrid types that are not concerned with cell size
        /// or subgrid tree index origin offset aspects
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGrid(ISubGridTree owner, ISubGrid parent, byte level)
        {
            // Assert there is an owning tree (things don't work well without one!)
            if (owner == null)
            {
              //  throw new ArgumentException("Owner cannot be null when creating a subgrid", "owner");
            }

            Owner = owner;
            Parent = parent;
            Level = level;
        }

        /// <summary>
        /// The number of on-the-ground cells that the span of this subgrid covers along each axis
        /// </summary>
        /// <returns></returns>
        public uint AxialCellCoverageByThisSubgrid() => (uint)SubGridTree.SubGridTreeDimension << ((Owner.NumLevels - Level) * SubGridTree.SubGridIndexBitsPerLevel);

        /// <summary>
        /// The number of on-the-ground cells that the span of a child subgrid of this subgrid covers along each axis
        /// </summary>
        public uint AxialCellCoverageByChildSubgrid() => AxialCellCoverageByThisSubgrid() >> SubGridTree.SubGridIndexBitsPerLevel;

        /// <summary>
        /// Sets the origin position of this subgrid to the supplied X and Y values within the cells of the parent subgrid. 
        /// This action locks the location of this subgrid in space with respect to the origin position of the parent subgrid.
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        public void SetOriginPosition(uint CellX, uint CellY)
        {
            if (Parent == null)
            { 
               throw new ArgumentException("Cannot set origin position without parent");
            }

            if (CellX >= SubGridTree.SubGridTreeDimension || CellY >= SubGridTree.SubGridTreeDimension)
            {
                throw new ArgumentException("Cell X, Y location is not in the valid cell address range for the subgrid");
            }

            OriginX = Parent.OriginX + CellX * AxialCellCoverageByThisSubgrid();
            OriginY = Parent.OriginY + CellY * AxialCellCoverageByThisSubgrid();
        }

        /// <summary>
        /// SetAbsoluteOriginPosition sets the origin position for this cell in terms
        /// of absolute cell origin coordinates.
        /// At the current time, it is only valid to do if the subgrid does not have a
        /// parent (in which case SetOriginPosition should be used);
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        public void SetAbsoluteOriginPosition(uint originX, uint originY)
        {
            if (Parent != null)
            {
                throw new Exception("Nodes referencing parent nodes may not have their origin modified");
            }

            OriginX = originX;
            OriginY = originY;
        }

        /// <summary>
        /// Determines the local in-subgrid X/Y location of a cell given its absolute cell index.
        /// This is a subgrid relative operation only, and depends only on the Owner to derive the difference
        /// between the numer of levels in the overall tree, and the level in the tree at which this subgrid resides 
        /// to compute the subgrid relative X and y cell indices as it is a leaf subgrid.
        /// WARNING: This call assumes the cell index does lie within this subgrid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        public void GetSubGridCellIndex(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY)
        {
            int SHRValue = (Owner.NumLevels - Level) * SubGridTree.SubGridIndexBitsPerLevel;
            SubGridX = (byte)((CellX >> SHRValue) & SubGridTree.SubGridLocalKeyMask);
            SubGridY = (byte)((CellY >> SHRValue) & SubGridTree.SubGridLocalKeyMask);

            //  Debug.Assert((SubGridX >=0) && (SubGridX < SubGridTree.SubGridTreeDimension) &
            //         (SubGridY >=0) && (SubGridY < SubGridTree.SubGridTreeDimension),
            //         "GetSubGridCellIndex given cell address out of bounds for this subgrid");
        }

        /// <summary>
        /// GetOTGLeafSubGridCellIndex determines the local in-subgrid X/Y location of a
        /// cell given its absolute cell index in an on-the-ground leaf subgrid where the level of the subgrid is implicitly known
        /// to be the same as FOwner.Numlevels. Do not call this method for a subgrid that is not a leaf subgrid
        /// WARNING: This call assumes the cell index does lie within this subgrid
        /// and (currently) no range checking is performed to ensure this}
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <param name="SubGridX"></param>
        /// <param name="SubGridY"></param>
        public void GetOTGLeafSubGridCellIndex(int CellX, int CellY, out byte SubGridX, out byte SubGridY)
        {
            SubGridX = (byte)(CellX & SubGridTree.SubGridLocalKeyMask);
            SubGridY = (byte)(CellY & SubGridTree.SubGridLocalKeyMask);

            //  Debug.Assert((SubGridX >=0) && (SubGridX < SubGridTree.SubGridTreeDimension) &
            //         (SubGridY >=0) && (SubGridY < SubGridTree.SubGridTreeDimension),
            //         "GetOTGLeafSubGridCellIndex given cell address out of bounds for this subgrid");
        }

        /// <summary>
        /// Determine if this subgrid represents a leaf subgrid containing information for on-the-ground cells
        /// </summary>
        public bool IsLeafSubGrid() => Level == Owner.NumLevels;

        /// <summary>
        /// Returns a moniker string comprised of the X and Y origin ordinates in the sub greid cell address space
        /// separated by a colon, eg: in the form 1234:5678
        /// </summary>
        public string Moniker() => string.Format("{0}:{1}", OriginX, OriginY);

        /// <summary>
        /// A virtual method representing an access mechanism to request a child subgrid at the X/Y location in this subgrid
        /// Note: By definition, leaf sub grids do not have child subgrids.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public virtual ISubGrid GetSubGrid(byte X, byte Y)
        {
            throw new Exception("SubGrid.GetSubGrid() should never be called");
        }

        /// <summary>
        /// A virtual method representing an access mechanism to request a child subgrid at the X/Y location in this subgrid
        /// Note: By definition, leaf sub grids do not have child subgrids.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual void SetSubGrid(byte X, byte Y, ISubGrid value)
        {
            throw new Exception("SubGrid.SetSubGrid() should never be called");
        }

        /// <summary>
        /// Calculates the location in the world coordinate/ system of the bottom left hand corner of the 
        /// bottom left hand on-the-ground corner of the bottom left hand on-the-ground cell in the grid
        /// </summary>
        /// <param name="WorldOriginX"></param>
        /// <param name="WorldOriginY"></param>
        public virtual void CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY)
        {
            WorldOriginX = ((int)OriginX - Owner.IndexOriginOffset) * Owner.CellSize;
            WorldOriginY = ((int)OriginY - Owner.IndexOriginOffset) * Owner.CellSize;
        }

        /// <summary>
        /// Clear sets all the entries in the grid to be unassigned, or null
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// AllChangesMigrated tells this subgrid that any changes that have been made to
        /// it (and which resulted in the dirty flag being set) have been migrated to
        /// another location. This essentially just sets the dirty flag to false, but
        /// encapsulates the semantics that any changes have been dealt with/preserved
        /// externally to this subgrid
        /// 
        /// </summary>
        public void AllChangesMigrated() => dirty = false;

        /// <summary>
        /// IsEmpty determines if this subgrid contains any information. By default the base 
        /// implementation is never empty
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEmpty() => false;

        /// <summary>
        /// RemoveFromParent removes the reference to this subgrid from the parent node
        /// subgrid. It does not free the subgrid, just removes it from the tree.
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent == null)
                return;

            Parent.GetSubGridCellIndex(OriginX, OriginY, out byte SubGridX, out byte SubGridY);
            Parent.SetSubGrid(SubGridX, SubGridY, null);
        }

        /// <summary>
        /// Determines if this subgrid contains the cell identified by an on-the-ground CellX and CellY location
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public bool ContainsOTGCell(uint CellX, uint CellY)
        {
           uint AxialCoverage = AxialCellCoverageByThisSubgrid();

           return (CellX >= OriginX) && (CellX < OriginX + AxialCoverage) && (CellY >= OriginY) && (CellY < OriginY + AxialCoverage);
        }

        /// <summary>
        /// CellHasValue indicates if the cell identified by CellX, CellY has a value (hence is not null)
        /// CellHasValue queries the leaf sub grid to determine if the cell at the
        /// given X/Y location within it has a value. CellX and CellY are in the
        /// 0..SubGridTreeDimension-1 coordinate space of the subgrid.
        /// WARNING: This is a comparitively expensive operation and so should not be used with abandon!
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        /// <returns></returns>
        public virtual bool CellHasValue(byte CellX, byte CellY)
        {
            throw new Exception("SubGrid.CellHasValue() should never be called");
        }

        /// <summary>
        /// Counts the number of cells that are non null in the subgrid using the base CellHasValue() interface
        /// </summary>
        /// <returns></returns>
        public virtual int CountNonNullCells()
        {
            int result = 0;

            for (int I = 0; I < SubGridTree.SubGridTreeCellsPerSubgrid; I++)
            {
                if (CellHasValue((byte)(I / SubGridTree.SubGridTreeDimension), (byte)(I % SubGridTree.SubGridTreeDimension)))
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
                throw new Exception("Nodes referencing parent nodes may not have their level modified");
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
            OriginX = reader.ReadUInt32();
            OriginY = reader.ReadUInt32();
        }

/*
        /// Write the contents of the Items array using the supplied writer
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public virtual void Write(BinaryFormatter formatter, Stream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public virtual void Read(BinaryFormatter formatter, Stream stream)
        {
            throw new NotImplementedException();
        }
*/

        /// <summary>
        /// Converts the subgrid origin cell location into a SubGridAddress identifying this subgrid
        /// </summary>
        /// <returns></returns>
        public SubGridCellAddress OriginAsCellAddress() => new SubGridCellAddress(OriginX, OriginY);

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Write(bw, new byte[10000]);

                    return ms.ToArray();
                }
            }
        }

        public byte[] ToBytes(byte[] helperBuffer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Write(bw, helperBuffer ?? new byte[10000]);

                    return ms.ToArray();
                }
            }
        }

        public byte[] ToBytes(MemoryStream helperStream, byte[] helperBuffer)
        {
            throw new NotImplementedException("Not done yet");
        }

        public void FromBytes(byte[] bytes, byte[] helperBuffer = null)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader bw = new BinaryReader(ms))
                {
                    Read(bw, helperBuffer ?? new byte[10000]);
                }
            }
        }
    }
}

