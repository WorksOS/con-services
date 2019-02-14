using System;
using System.IO;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  /// <summary>
  /// Interface defining basic interface methods for the base sub grid type underlying all sub grid types
  /// </summary>
  public interface ISubGrid
  {
    /// <summary>
    /// ‘Level’ in the sub grid tree in which this sub grid resides. Level 1 is the root node in the tree
    /// </summary>
    byte Level { get; set; }

    /// <summary>
    /// Grid cell X Origin of the bottom left hand cell in this sub grid. 
    /// Origin is wrt to cells of the spatial dimension held by this sub grid
    /// </summary>
    uint OriginX { get; set; }

    /// <summary>
    /// Grid cell Y Origin of the bottom left hand cell in this sub grid. 
    /// Origin is wrt to cells of the spatial dimension held by this sub grid
    /// </summary>
    uint OriginY { get; set; }

    /// <summary>
    /// Dirty property used to indicate the presence of changes that are not persisted.
    /// </summary>
    bool Dirty { get; }

    /// <summary>
    /// The owning sub grid tree that this su grid is a part of.
    /// </summary>
    ISubGridTree Owner { get; set; }

    /// <summary>
    /// The parent sub grid that owns this sub grid as a cell.
    /// </summary>
    ISubGrid Parent { get; set; }

    void SetDirty();

    uint AxialCellCoverageByThisSubGrid();
    uint AxialCellCoverageByChildSubGrid();
    bool ContainsOTGCell(uint CellX, uint CellY);
    void SetOriginPosition(uint CellX, uint CellY);
    void GetSubGridCellIndex(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY);
    bool IsLeafSubGrid();
    string Moniker();
    ISubGrid GetSubGrid(int X, int Y);
    void SetSubGrid(int X, int Y, ISubGrid value);
    void CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);
    void Clear();
    void AllChangesMigrated();
    bool IsEmpty();
    void RemoveFromParent();
    bool CellHasValue(byte CellX, byte CellY);
    int CountNonNullCells();
    void SetAbsoluteOriginPosition(uint originX, uint originY);
    void SetAbsoluteLevel(byte level);

    void Write(BinaryWriter writer, byte[] buffer);

    void Read(BinaryReader reader, byte[] buffer);

    SubGridCellAddress OriginAsCellAddress();

    byte[] ToBytes();
    byte[] ToBytes(byte[] helperBuffer);
    void FromBytes(byte[] bytes, byte[] helperBuffer = null);

    /// <summary>
    /// Perform an action over all cells in the sub grid by calling the supplied action lambda with the coordinates of each cell
    /// </summary>
    /// <param name="action"></param>
    void ForEach(Action<byte, byte> action);
    }
}
