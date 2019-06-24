using System.IO;

namespace VSS.TRex.Common.Records
{
  public struct PatchOffsetsRecord
  {
    /// <summary>
    /// Elevation offset in millimeters.
    /// </summary>
    public uint ElevationOffset;

    /// <summary>
    /// Time offset in seconds.
    /// </summary>
    public uint TimeOffset;

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="elevationOffset"></param>
    /// <param name="timeOffset"></param>
    public PatchOffsetsRecord(uint elevationOffset, uint timeOffset)
    {
      ElevationOffset = elevationOffset;
      TimeOffset = timeOffset;
    }

    /// <summary>
    /// Initialises the Min and Max properties with null values.
    /// </summary>
    public void Clear()
    {
      ElevationOffset = uint.MinValue;
      TimeOffset = uint.MinValue;
    }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ElevationOffset);
      writer.Write(TimeOffset);
    }

    /// <summary>
    /// Serialises content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      ElevationOffset = reader.ReadUInt32();
      TimeOffset = reader.ReadUInt32();
    }
  }
}
