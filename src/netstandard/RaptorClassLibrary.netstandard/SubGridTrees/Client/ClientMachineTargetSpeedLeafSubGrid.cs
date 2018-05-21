using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Cells;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
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
		/// Determine if a filtered machine speed targets value is valid (not null)
		/// </summary>
		/// <param name="filteredValue"></param>
		/// <returns></returns>
		public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.MachineSpeed == CellPass.NullMachineSpeed;

		/// <summary>
		/// Assign filtered machine speed targets value from a filtered pass to a cell
		/// </summary>
		/// <param name="cellX"></param>
		/// <param name="cellY"></param>
		/// <param name="Context"></param>
		public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
		{
			Cells[cellX, cellY].Min = Context. FilteredValue.FilteredPassData.FilteredPass.MachineSpeed;
		}

	}
}
