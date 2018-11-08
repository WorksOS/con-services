using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.Types
{
  public struct PassCountRangeRecord
  {
    /// <summary>
    /// Minimum Pass Count range value.
    /// </summary>
    public ushort Min { get; set; }

    /// <summary>
    /// Maximum Pass Count range value.
    /// </summary>
    public ushort Max { get; set; }

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public PassCountRangeRecord(ushort min, ushort max)
    {
      Min = min;
      Max = max;
    }

    /// <summary>
    /// Initialises the Min and Max properties with null values.
    /// </summary>
    public void Clear()
    {
      Min = CellPassConsts.NullPassCountValue;
      Max = CellPassConsts.NullPassCountValue;
    }

    /// <summary>
    /// Initialises the Min and Max properties with values.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void SetMinMax(ushort min, ushort max)
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
  }
}
