using System;
using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Rendering.Palettes
{
  public class HeightPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private double MinElevation;
    private double MaxElevation;
    private double ElevationPerBand;

    public Color[] ElevationPalette =
    {
      Color.Aqua,
      Color.Yellow,
      Color.Fuchsia,
      Color.Lime,
      Color.FromArgb(0x80, 0x80, 0xFF),
      Color.LightGray,
      Color.FromArgb(0xEB, 0xFD, 0xAC),
      Color.FromArgb(0xFF, 0x80, 0x00),
      Color.FromArgb(0xFF, 0xC0, 0xFF),
      Color.FromArgb(0x96, 0xCB, 0xFF),
      Color.FromArgb(0xB5, 0x8E, 0x6C),
      Color.FromArgb(0xFF, 0xFF, 0x80),
      Color.FromArgb(0xFF, 0x80, 0x80),
      Color.FromArgb(0x80, 0xFF, 0x00),
      Color.FromArgb(0x00, 0x80, 0xFF),
      Color.FromArgb(0xFF, 0x00, 0x80),
      Color.Teal,
      Color.FromArgb(0xFF, 0xC0, 0xC0),
      Color.FromArgb(0xFF, 0x80, 0xFF),
      Color.FromArgb(0x00, 0xFF, 0x80)
    };

    public HeightPalette() : base(null)
    { }

    public HeightPalette(double minElevation, double maxElevation) : base(null)
    {
      MinElevation = minElevation;
      MaxElevation = maxElevation;
      ElevationPerBand = (MaxElevation - MinElevation) / ElevationPalette.Length;
    }

    public Color ChooseColour(double value)
    {
      var color = Color.Black;

      if (value != Consts.NullDouble)
      {
        int index = (int)Math.Floor((value - MinElevation) / ElevationPerBand);
        color = Range.InRange(index, 0, ElevationPalette.Length - 1) ? ElevationPalette[index] : Color.Black; // Color.Empty;
      }

      return color;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(MinElevation);
      writer.WriteDouble(MaxElevation);
      writer.WriteDouble(ElevationPerBand);

      writer.WriteBoolean(ElevationPalette != null);

      if (ElevationPalette != null)
      {
        writer.WriteInt(ElevationPalette.Length);

        foreach (var color in ElevationPalette)
          writer.WriteInt(color.ToArgb());
      }
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      MinElevation = reader.ReadDouble();
      MaxElevation = reader.ReadDouble();
      ElevationPerBand = reader.ReadDouble();

      if (reader.ReadBoolean())
      {
        var numberOfColors = reader.ReadInt();

        ElevationPalette = new Color[numberOfColors];

        for (var i= 0; i < ElevationPalette.Length; i++)
          ElevationPalette[i] = Color.FromArgb(reader.ReadInt());
      }
    }

  }
}
