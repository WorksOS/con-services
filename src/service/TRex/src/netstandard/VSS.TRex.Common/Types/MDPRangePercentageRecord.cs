using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Types
{
  public struct MDPRangePercentageRecord
  {
    /// <summary>
    /// Minimum MDP percentage range value.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Maximum MDP percentage range value.
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Constractor with arguments.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public MDPRangePercentageRecord(double min, double max)
    {
      Min = min;
      Max = max;
    }

    /// <summary>
    /// Initialises the Min and Max properties with null values.
    /// </summary>
    public void Clear()
    {
      Min = Consts.NullDouble;
      Max = Consts.NullDouble;
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
      Min = reader.ReadDouble();
      Max = reader.ReadDouble();
    }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Min);
      writer.WriteDouble(Max);
    }

    /// <summary>
    /// Serialises content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Min = reader.ReadDouble();
      Max = reader.ReadDouble();
    }
  }
}
