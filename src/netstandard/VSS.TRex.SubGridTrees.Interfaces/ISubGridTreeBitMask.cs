using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface ISubGridTreeBitMask : ISubGridTree
  {
    /// <summary>
    /// Performs the fundamental GetCell operation that returns a boolean value noting the state of the 
    /// bit in the tree at the [CellX, CellY] location
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>                        
    bool GetCell(uint CellX, uint CellY);

    /// <summary>
    /// Performs the fundamental SetCell operation that sets the state of bit in the tree at the 
    /// [CellX, CellY] location according to the boolean value parameter
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <param name="Value"></param>
    /// <returns></returns>     
    void SetCell(uint CellX, uint CellY, bool Value);

    /// <summary>
    /// Determines if there is an existing leaf containing the requested bit, and if that bit it set to 1 (true)
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool GetLeaf(uint CellX, uint CellY);

    /// <summary>
    /// Default array indexer for the bits in the subgrid tree mask
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool this[uint CellX, uint CellY] { get; set; }

    /// <summary>
    /// RemoveLeafOwningCell locates the leaf subgrid that contains the OTG cell identified by CellX and CellY and removes it from the
    /// sub grid tree.        
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    void RemoveLeafOwningCell(uint CellX, uint CellY);

    /// <summary>
    /// CountBits performs a scan of the subgrid bit mask tree and counts all the bits that are set within it
    /// </summary>
    /// <returns></returns>
    long CountBits();

    /// <summary>
    /// Calculates the world coordinate bounding rectangle within the bit mask subgrid that encloses all bits that
    /// are set to 1 (true)
    /// </summary>
    /// <returns></returns>
    BoundingWorldExtent3D ComputeCellsWorldExtents();

    /// <summary>
    /// LeafExists determines if there is a leaf cell in the sub grid tree that contains the cell at address [CellX, CellY].
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool LeafExists(uint CellX, uint CellY);

    /// <summary>
    /// Takes a source SubGridBitMask instance and performs a bitwise OR of the contents of source against the
    /// contents of this instance, modifying the state of this subgrid bit mask tree to produce the result
    /// </summary>
    /// <param name="Source"></param>
    void SetOp_OR(ISubGridTreeBitMask Source);

    /// <summary>
    /// Takes a source SubGridBitMask instance and performs a bitwise AND of the contents of source against the
    /// contents of this instance, modifying the state of this subgrid bit mask tree to produce the result
    /// </summary>
    /// <param name="Source"></param>
    void SetOp_AND(ISubGridTreeBitMask Source);

    /// <summary>
    ///  ClearCellIfSet will set the value of a cell to false if the current
    /// value of cell is True. The function returns true if the cell was set
    /// and has been cleared
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool ClearCellIfSet(uint CellX, uint CellY);

    /// <summary>
    /// Scan all the bits in the bit mask subgrid tree treating each set bit as the address of a subgrid
    /// call the supplied Action 'functor' with a leaf subgrid origin address calculated from each of the bits
    /// Note: As each bit represents an on-the-ground leaf subgrid, cell address of that bit needs to be transformed
    /// from the level 5 (node) layer to the level 6 (leaf) layer
    /// </summary>
    /// <param name="functor"></param>
    void ScanAllSetBitsAsSubGridAddresses(Action<ISubGridCellAddress> functor);

    ISubGrid CreateNewSubgrid(byte level);

    /// <summary>
    /// GetCellExtents computes the real world extents of the OTG cell identified
    /// by X and Y. X and Y are in the bottom left origin of the grid.
    /// The returned extents are translated to the centered origin of the real
    /// world coordinate system
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    void GetCellExtents(uint X, uint Y, ref BoundingWorldExtent3D extents);
  }
}
