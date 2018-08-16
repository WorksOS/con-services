using System.IO;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.Types
{
  public struct TemperatureWarningLevelsRecord
  {
		/// <summary>
		/// Minimum temperature warning levels value.
		/// </summary>
		public ushort Min { get; set; }
		/// <summary>
		/// Maximum temperature warning levels value.
		/// </summary>
		public ushort Max { get; set; }

		/// <summary>
		/// Constractor with arguments.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public TemperatureWarningLevelsRecord(ushort min, ushort max)
	  {
		  Min = min;
		  Max = max;
	  }

		/// <summary>
		/// Initialises the Min and Max properties with null values.
		/// </summary>
	  public void Clear()
	  {
		  Min = CellPassConsts.NullMaterialTemperatureValue;
		  Max = CellPassConsts.NullMaterialTemperatureValue;
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

    public bool Equal(TemperatureWarningLevelsRecord other)
    {
      return Min == other.Min && Max == other.Max;
    }
  }
}
