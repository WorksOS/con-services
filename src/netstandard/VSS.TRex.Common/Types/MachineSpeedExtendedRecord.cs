using System.IO;
using VSS.TRex.Common.CellPasses;

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
	  /// Initialises the Min and Max properties with null values.
	  /// </summary>
	  public void Clear()
	  {
	    Min = CellPassConsts.NullMachineSpeed;
	    Max = CellPassConsts.NullMachineSpeed;
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

	  /// <summary>
	  /// Defines a publically accessible null value for this cell value type
	  /// </summary>
	  public static MachineSpeedExtendedRecord NullValue = MachineSpeedExtendedRecord.Null();

	  /// <summary>
	  /// Implements the business logic to create the null value for this cell valuye type
	  /// </summary>
	  /// <returns></returns>
	  public static MachineSpeedExtendedRecord Null()
	  {
	    MachineSpeedExtendedRecord Result = new MachineSpeedExtendedRecord();
	    Result.Clear();
	    return Result;
	  }

	  public bool Equals(MachineSpeedExtendedRecord other)
	  {
	    return Min == other.Min && Max == other.Max;
	  }
  }
}
