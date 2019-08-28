using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Common.Records
{
  public struct TemperatureWarningLevelsRecord
  {
    private const byte USHORT_TYPES_COUNT = 2;

    /// <summary>
    /// Minimum temperature warning levels value.
    /// </summary>
    public ushort Min;

    /// <summary>
    /// Maximum temperature warning levels value.
    /// </summary>
    public ushort Max;

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public static int IndicativeSizeInBytes() => USHORT_TYPES_COUNT * sizeof(ushort);

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public TemperatureWarningLevelsRecord(ushort min, ushort max)
	  {
		  Min = min;
		  Max = max;
	  }

		/// <summary>
		/// Initializes the Min and Max properties with null values.
		/// </summary>
	  public void Clear()
	  {
		  Min = CellPassConsts.NullMaterialTemperatureValue;
		  Max = CellPassConsts.NullMaterialTemperatureValue;
		}

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(Min);
      writer.Write(Max);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      Min = reader.ReadUInt16();
      Max = reader.ReadUInt16();
    }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteInt(Min);
      writer.WriteInt(Max);
    }

    /// <summary>
    /// Serialises content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Min = (ushort)reader.ReadInt();
      Max = (ushort)reader.ReadInt();
    }

    public bool Equals(TemperatureWarningLevelsRecord other)
    {
      return Min == other.Min && Max == other.Max;
    }
  }
}
