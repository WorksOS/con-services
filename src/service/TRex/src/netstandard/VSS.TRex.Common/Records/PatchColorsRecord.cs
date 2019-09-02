using System.Drawing;
using System.IO;

namespace VSS.TRex.Common.Records
{
  public struct PatchColorsRecord
  {
    /// <summary>
    /// Elevation offset in millimeters.
    /// </summary>
    public uint ElevationOffset;

    /// <summary>
    /// Color
    /// </summary>
    public Color Colour;

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="elevationOffset"></param>
    /// <param name="colour"></param>
    public PatchColorsRecord(uint elevationOffset, Color colour)
    {
      ElevationOffset = elevationOffset;
      Colour = colour;
    }

    /// <summary>
    /// Initialises the properties with null values.
    /// </summary>
    public void Clear()
    {
      ElevationOffset = uint.MinValue;
      Colour = Color.Empty;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ElevationOffset);
      writer.Write(Colour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      ElevationOffset = reader.ReadUInt32();
      Colour = Color.FromArgb(reader.ReadInt32());
    }
  }
}
