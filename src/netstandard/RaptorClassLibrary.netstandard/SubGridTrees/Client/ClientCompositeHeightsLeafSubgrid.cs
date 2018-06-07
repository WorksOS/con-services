using System.IO;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// A client subgrid storing composite height information for each cell in the subgrid.
  /// </summary>
  public class ClientCompositeHeightsLeafSubgrid : GenericClientLeafSubGrid<SubGridCellCompositeHeightsRecord>
  {
    /// <summary>
    /// Initilise the null cell values for the client subgrid
    /// </summary>
    static ClientCompositeHeightsLeafSubgrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y].Clear());
    }

    /// <summary>
    /// Constructor. Set the grid to HeightAndTime.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCompositeHeightsLeafSubgrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize,
      uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      _gridDataType = TRex.Types.GridDataType.CompositeHeights;
    }

    /// <summary>
    /// Determines if the height at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY)
    {
      return Cells[cellX, cellY].LowestHeight != Consts.NullHeight ||
             Cells[cellX, cellY].HighestHeight != Consts.NullHeight ||
             Cells[cellX, cellY].LastHeight != Consts.NullHeight ||
             Cells[cellX, cellY].FirstHeight != Consts.NullHeight;
    }

    /// <summary>
    /// Zero out all elevations
    /// </summary>
    public void SetToZeroHeight()
    {
      SubGridUtilities.SubGridDimensionalIterator((i, j) =>
      {
        Cells[i, j].LowestHeight = 0;
        Cells[i, j].HighestHeight = 0;
        Cells[i, j].LastHeight = 0;
        Cells[i, j].FirstHeight = 0;
      });
    }

    /// <summary>
    /// Null out all elevations
    /// </summary>
    public void SetHeightsToNull()
    {
      SubGridUtilities.SubGridDimensionalIterator((i, j) =>
      {
        Cells[i, j].LowestHeight = Consts.NullHeight;
        Cells[i, j].HighestHeight = Consts.NullHeight;
        Cells[i, j].LastHeight = Consts.NullHeight;
        Cells[i, j].FirstHeight = Consts.NullHeight;
      });
    }

    /// <summary>
    /// Clears all cells in the composite height grid to null heights and dates
    /// </summary>
    public override void Clear()
    {
      base.Clear();
    } 

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }
  }
}
