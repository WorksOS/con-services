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
    bool GetCell(int CellX, int CellY);

    /// <summary>
    /// Performs the fundamental SetCell operation that sets the state of bit in the tree at the 
    /// [CellX, CellY] location according to the boolean value parameter
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <param name="Value"></param>
    /// <returns></returns>     
    void SetCell(int CellX, int CellY, bool Value);

    /// <summary>
    /// Default array indexer for the bits in the sub grid tree mask
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool this[int CellX, int CellY] { get; set; }

    /// <summary>
    /// RemoveLeafOwningCell locates the leaf sub grid that contains the OTG cell identified by CellX and CellY and removes it from the
    /// sub grid tree.        
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    void RemoveLeafOwningCell(int CellX, int CellY);

    /// <summary>
    /// CountBits performs a scan of the sub grid bit mask tree and counts all the bits that are set within it
    /// </summary>
    /// <returns></returns>
    long CountBits();

    /// <summary>
    /// Calculates the world coordinate bounding rectangle within the bit mask sub grid that encloses all bits that
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
    bool LeafExists(int CellX, int CellY);

    /// <summary>
    /// Takes a source SubGridBitMask instance and performs a bitwise OR of the contents of source against the
    /// contents of this instance, modifying the state of this sub grid bit mask tree to produce the result
    /// </summary>
    /// <param name="Source"></param>
    void SetOp_OR(ISubGridTreeBitMask Source);

    /// <summary>
    /// Takes a source SubGridBitMask instance and performs a bitwise AND of the contents of source against the
    /// contents of this instance, modifying the state of this sub grid bit mask tree to produce the result
    /// </summary>
    /// <param name="Source"></param>
    void SetOp_AND(ISubGridTreeBitMask Source);

    /// <summary>
    /// Takes a source SubGridBitMask instance and performs a bitwise XOR of the contents of source against the
    /// contents of this instance, modifying the state of this sub grid bit mask tree to produce the result
    /// </summary>
    /// <param name="Source"></param>

    void SetOp_XOR(ISubGridTreeBitMask Source);

    /// <summary>
    ///  ClearCellIfSet will set the value of a cell to false if the current
    /// value of cell is True. The function returns true if the cell was set
    /// and has been cleared
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    /// <returns></returns>
    bool ClearCellIfSet(int CellX, int CellY);

    /// <summary>
    /// Scan all the bits in the bit mask sub grid tree treating each set bit as the address of a sub grid
    /// call the supplied Action 'functor' with a leaf sub grid origin address calculated from each of the bits
    /// Note: As each bit represents an on-the-ground leaf sub grid, cell address of that bit is transformed
    /// from the level 5 (node) layer to the level 6 (leaf) layer
    /// </summary>
    /// <param name="functor"></param>
    void ScanAllSetBitsAsSubGridAddresses(Action<SubGridCellAddress> functor);

    ISubGrid CreateNewSubGrid(byte level);
  }
}
