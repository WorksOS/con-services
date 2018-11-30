using System.IO;

namespace VSS.TRex.Common.Types
{
  public struct PatchOffsetsRecord
  {
    /// <summary>
    /// Elevatin offset in millimeters.
    /// </summary>
    public uint ElevationOffset { get; set; }

    /// <summary>
    /// Time offset in seconds.
    /// </summary>
    public uint TimeOffset { get; set; }

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
    /// Serialises comtent of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      ElevationOffset = reader.ReadUInt32();
      TimeOffset = reader.ReadUInt32();
    }

  }
}
