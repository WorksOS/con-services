using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VSS.TRex.Common;

namespace VSS.TRex.Types
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

	  /// <summary>
	  /// Serialises content of the cell to the writer
	  /// </summary>
	  /// <param name="writer"></param>
	  public void Write(BinaryWriter writer)
	  {
	    writer.Write(Min);
	    writer.Write(Max);
	  }

    /// <summary>
    /// Serialises comtent of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
	  {
	    Min = reader.ReadUInt16();
	    Max = reader.ReadUInt16();
	  }
  }
}
