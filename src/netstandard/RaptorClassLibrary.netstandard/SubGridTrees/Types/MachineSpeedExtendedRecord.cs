using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Common;

namespace VSS.TRex.SubGridTrees.Types
{
	/// <summary>
	/// Contains minimum and maximum machine speed values.
	/// </summary>
	public struct MachineSpeedExtendedRecord
	{
		/// <summary>
		/// Minimum machine speed value.
		/// </summary>
		public ushort Min { get; set; }
	  /// <summary>
	  /// Maximum machine speed value.
	  /// </summary>
	  public ushort Max { get; set; }

		/// <summary>
		/// Constractor with arguments.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public MachineSpeedExtendedRecord(ushort min, ushort max)
		{
			Min = min;
			Max = max;
		}
	}
}
