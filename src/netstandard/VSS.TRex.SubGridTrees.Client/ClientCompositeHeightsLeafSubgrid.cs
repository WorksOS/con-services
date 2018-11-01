using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// A client subgrid storing composite height information for each cell in the subgrid.
  /// </summary>
  public class ClientCompositeHeightsLeafSubgrid : GenericClientLeafSubGrid<SubGridCellCompositeHeightsRecord>
  {
    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientCompositeHeightsLeafSubgrid()
    {
       ForEachStatic((x, y) => NullCells[x, y] = SubGridCellCompositeHeightsRecord.NullValue);
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.CompositeHeights;
    }

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientCompositeHeightsLeafSubgrid()
    {
      Initialise();
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
      Initialise();
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
    public void SetToZeroHeight() => ForEach((i, j) => Cells[i, j].SetToZeroHeight());

    /// <summary>
    /// Null out all elevations
    /// </summary>
    public void SetHeightsToNull() => ForEach((i, j) => Cells[i, j] = SubGridCellCompositeHeightsRecord.NullValue);

    /// <summary>
    /// Provides a copy of the null value defined for cells in the client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override SubGridCellCompositeHeightsRecord NullCell() => SubGridCellCompositeHeightsRecord.NullValue;

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<SubGridCellCompositeHeightsRecord> _other = (IGenericClientLeafSubGrid<SubGridCellCompositeHeightsRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      ForEach((x, y) => Cells[x, y] = new SubGridCellCompositeHeightsRecord
      {
        LowestHeight = x,
        FirstHeight = y,
        LowestHeightTime = x + y,
        HighestHeight = 2 * (x + y),        
      });
    }
    
    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      ForEach((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      ForEach((x, y) => Cells[x, y].Read(reader));
    }

    /// <summary>
    /// Determine if a filtered cell pass height is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) =>
      filteredValue.FilteredPass.Height == CellPassConsts.NullHeight;

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      throw new System.NotImplementedException();
    }
  }
}
