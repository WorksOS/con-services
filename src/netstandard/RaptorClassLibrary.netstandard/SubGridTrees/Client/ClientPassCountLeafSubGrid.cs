using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a Pass Count client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientPassCountLeafSubGrid : GenericClientLeafSubGrid<SubGridCellPassDataPassCountEntryRecord>
  {
    /// <summary>
    /// First pass map records which cells hold cell pass Pass Counts that were derived
    /// from the first pass a machine made over the corresponding cell
    /// </summary>
    public SubGridTreeBitmapSubGridBits FirstPassMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Initilise the null cell values for the client subgrid
    /// </summary>
    static ClientPassCountLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = SubGridCellPassDataPassCountEntryRecord.NullValue);
    }

    /// <summary>
    /// Pass Count subgrids require lift processing...
    /// </summary>
    /// <returns></returns>
    public override bool WantsLiftProcessingResults() => true;

    /// <summary>
    /// Constructor. Set the grid to Pass Count.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientPassCountLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      EventPopulationFlags |= PopulationControlFlags.WantsTargetPassCountValues;

      _gridDataType = GridDataType.PassCount;
    }

    /// <summary>
    /// Determines if the Pass Count at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].MeasuredPassCount != CellPass.NullPassCountValue;

    /// <summary>
    /// Sets all cell Pass Counts to null and clears the first pass and sureyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      FirstPassMap.Clear();
    }

    /// <summary>
    /// Assign filtered Pass Count value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="Context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      Cells[cellX, cellY].MeasuredPassCount = (ushort) Context.FilteredValue.PassCount;
      Cells[cellX, cellY].TargetPassCount = Context.FilteredValue.FilteredPassData.TargetValues.TargetPassCount;
    }

    /// <summary>
    /// Determine if a filtered CMV is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => false;

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      ForEach((x, y) => Cells[x, y] = new SubGridCellPassDataPassCountEntryRecord { MeasuredPassCount = (ushort) (x + 1), TargetPassCount = (ushort) (y + 1) });
    }

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<SubGridCellPassDataPassCountEntryRecord> _other = (IGenericClientLeafSubGrid<SubGridCellPassDataPassCountEntryRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    /// <summary>
    /// Provides a copy of the null value defined for cells in thie client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override SubGridCellPassDataPassCountEntryRecord NullCell() => SubGridCellPassDataPassCountEntryRecord.NullValue;

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      FirstPassMap.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      FirstPassMap.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }

  }
}
