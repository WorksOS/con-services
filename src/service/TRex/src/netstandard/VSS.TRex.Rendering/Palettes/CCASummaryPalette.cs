using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CCA summary data
  /// </summary>
  public class CCASummaryPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The color, which CCA summary data displayed in on a plan view map, where the material is over compacted.
    /// </summary>
    public Color OvercompactedColour = Color.Yellow;

    /// <summary>
    /// The color, which CCA summary data displayed in on a plan view map, where the material is compacted.
    /// </summary>
    public Color CompactedColour = Color.Red;

    /// <summary>
    /// The color, which CCA summary data displayed in on a plan view map, where the material is under compacted.
    /// </summary>
    public Color UndercompactedColour = Color.Aqua;


    public CCASummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(SubGridCellPassDataCCAEntryRecord ccaCellData)
    {
      if (ccaCellData.IsUndercompacted)
        return UndercompactedColour;

      if (ccaCellData.IsOvercompacted)
        return OvercompactedColour;

      return CompactedColour;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(OvercompactedColour.ToArgb());
      writer.WriteInt(CompactedColour.ToArgb());
      writer.WriteInt(UndercompactedColour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OvercompactedColour = Color.FromArgb(reader.ReadInt());
      CompactedColour = Color.FromArgb(reader.ReadInt());
      UndercompactedColour = Color.FromArgb(reader.ReadInt());
    }
  }
}
