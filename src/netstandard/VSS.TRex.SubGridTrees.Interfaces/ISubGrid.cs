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
    /// ‘Level’ in the subgridtree in which this subgrid resides. Level 1 is the root node in the tree
    /// </summary>
    byte Level { get; set; }

    /// <summary>
    /// Grid cell X Origin of the bottom left hand cell in this subgrid. 
    /// Origin is wrt to cells of the spatial dimension held by this subgrid
    /// </summary>
    uint OriginX { get; set; }

    /// <summary>
    /// Grid cell Y Origin of the bottom left hand cell in this subgrid. 
    /// Origin is wrt to cells of the spatial dimension held by this subgrid
    /// </summary>
    uint OriginY { get; set; }

    /// <summary>
    /// Dirty property used to indicate the presence of changes that are not persisted.
    /// </summary>
    bool Dirty { get; set; }

    /// <summary>
    /// The owning subgrid tree that this subgrid is a part of.
    /// </summary>
    ISubGridTree Owner { get; set; }

    /// <summary>
    /// The parent subgrid that owns this subgrid as a cell.
    /// </summary>
    ISubGrid Parent { get; set; }

    uint AxialCellCoverageByThisSubgrid();
    uint AxialCellCoverageByChildSubgrid();
    bool ContainsOTGCell(uint CellX, uint CellY);
    void SetOriginPosition(uint CellX, uint CellY);
    void GetSubGridCellIndex(uint CellX, uint CellY, out byte SubGridX, out byte SubGridY);
    bool IsLeafSubGrid();
    string Moniker();
    ISubGrid GetSubGrid(byte X, byte Y);
    void SetSubGrid(byte X, byte Y, ISubGrid value);
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

    ISubGridCellAddress OriginAsCellAddress();

    byte[] ToBytes();
    byte[] ToBytes(byte[] helperBuffer);
    void FromBytes(byte[] bytes, byte[] helperBuffer = null);

    /// <summary>
    /// Perform an action over all cells in the subgrid by calling the supplied action lambda with the coordinates of each cell
    /// </summary>
    /// <param name="action"></param>
    void ForEach(Action<byte, byte> action);
    }
}
