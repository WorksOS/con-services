using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  public class ClientMachineTargetSpeedLeafSubGrid : GenericClientLeafSubGrid<MachineSpeedExtendedRecord>
	{
		/// <summary>
		/// First pass map records which cells hold cell pass machine speed targets that were derived
		/// from the first pass a machine made over the corresponding cell
		/// </summary>
		public SubGridTreeBitmapSubGridBits FirstPassMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

	  /// <summary>
	  /// Initilise the null cell values for the client subgrid
	  /// </summary>
	  static ClientMachineTargetSpeedLeafSubGrid()
	  {
	    SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = MachineSpeedExtendedRecord.NullValue);
    }

		/// <summary>
    /// Constructor. Set the grid to MachineSpeedTarget.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientMachineTargetSpeedLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
		{
			_gridDataType = GridDataType.MachineSpeedTarget;
		}

	  /// <summary>
	  /// Speed target subgrids require lift processing...
	  /// </summary>
	  /// <returns></returns>
	  public override bool WantsLiftProcessingResults() => true;

    /// <summary>
    /// Determine if a filtered machine speed targets value is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.MachineSpeed == CellPassConsts.NullMachineSpeed;

		/// <summary>
		/// Assign filtered machine speed targets value from a filtered pass to a cell
		/// </summary>
		/// <param name="cellX"></param>
		/// <param name="cellY"></param>
		/// <param name="Context"></param>
		public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
		{
			Cells[cellX, cellY].Min = ((IProfileCell)Context.CellProfile).CellMinSpeed;
		  Cells[cellX, cellY].Max = ((IProfileCell)Context.CellProfile).CellMaxSpeed;
    }

	  /// <summary>
	  /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
	  /// </summary>
	  public override void FillWithTestPattern()
	  {
	    ForEach((x, y) => { Cells[x, y] = new MachineSpeedExtendedRecord {Min = x, Max = (ushort)(x + y)};});
	  }

	  /// <summary>
	  /// Determines if the machine speed at the cell location is null or not.
	  /// </summary>
	  /// <param name="cellX"></param>
	  /// <param name="cellY"></param>
	  /// <returns></returns>
	  public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].Min != CellPassConsts.NullMachineSpeed || Cells[cellX, cellY].Max != CellPassConsts.NullMachineSpeed;

	  /// <summary>
	  /// Provides a copy of the null value defined for cells in thie client leaf subgrid
	  /// </summary>
	  /// <returns></returns>
	  public override MachineSpeedExtendedRecord NullCell() => MachineSpeedExtendedRecord.NullValue;

	  /// <summary>
	  /// Sets all min/max cell machine speeds to null and clears the first pass and sureyed surface pass maps
	  /// </summary>
	  public override void Clear()
	  {
	    base.Clear();

	    FirstPassMap.Clear();
	  }

	  /// <summary>
	  /// Dumps machine speeds from subgrid to the log
	  /// </summary>
	  /// <param name="title"></param>
	  public override void DumpToLog(string title)
	  {
	    base.DumpToLog(title);
	  }

	  /// <summary>
	  /// Determines if the leaf content of this subgrid is equal to 'other'
	  /// </summary>
	  /// <param name="other"></param>
	  /// <returns></returns>
	  public override bool LeafContentEquals(IClientLeafSubGrid other)
	  {
	    bool result = true;

	    IGenericClientLeafSubGrid<MachineSpeedExtendedRecord> _other = (IGenericClientLeafSubGrid<MachineSpeedExtendedRecord>)other;
	    ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

	    return result;
	  }

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
