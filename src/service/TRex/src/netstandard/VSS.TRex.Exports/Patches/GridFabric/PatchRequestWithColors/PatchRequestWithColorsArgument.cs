using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequest;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors
{
  public class PatchRequestWithColorsArgument : PatchRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public bool RenderValuesToColours { get; set; }

    public IPlanViewPalette ColourPalette { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(RenderValuesToColours);

      writer.WriteBoolean(ColourPalette != null);
      ColourPalette?.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      RenderValuesToColours = reader.ReadBoolean();

      if (reader.ReadBoolean())
      {
        ColourPalette = TileRenderRequestArgumentPaletteFactory.GetPalette(Mode);
        ColourPalette.FromBinary(reader);
      }
    }
  }
}
