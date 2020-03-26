using System;
using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.Rendering.Palettes
{
  public class HeightPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;
    private readonly Color UndefinedColor = Color.Black;

    private double MinElevation;
    private double MaxElevation;
    private double ElevationPerBand;

    public Color[] ElevationPalette =
    {
      Color.FromArgb(200,0,0),
      Color.FromArgb(255,0,0),
      Color.FromArgb(225,60,0),
      Color.FromArgb(255,90,0),
      Color.FromArgb(255,130,0),
      Color.FromArgb(255,170,0),
      Color.FromArgb(255,200,0),
      Color.FromArgb(255,220,0),
      Color.FromArgb(250,230,0),
      Color.FromArgb(220,230,0),
      Color.FromArgb(210,230,0),
      Color.FromArgb(200,230,0),
      Color.FromArgb(180,230,0),
      Color.FromArgb(150,230,0),
      Color.FromArgb(130,230,0),
      Color.FromArgb(100,240,0),
      Color.FromArgb(0,255,0),
      Color.FromArgb(0,240,100),
      Color.FromArgb(0,230,130),
      Color.FromArgb(0,230,150),
      Color.FromArgb(0,230,180),
      Color.FromArgb(0,230,200),
      Color.FromArgb(0,230,210),
      Color.FromArgb(0,220,220),
      Color.FromArgb(0,200,230),
      Color.FromArgb(0,180,240),
      Color.FromArgb(0,150,245),
      Color.FromArgb(0,120,250),
      Color.FromArgb(0,90,255),
      Color.FromArgb(0,70,255),
      Color.FromArgb(0,0,255)
    };

    public HeightPalette() : base(null)
    { }

    public HeightPalette(double minElevation, double maxElevation) : base(null)
    {
      MinElevation = minElevation;
      MaxElevation = maxElevation;
      ElevationPerBand = (MaxElevation - MinElevation) / (ElevationPalette.Length - 1);
    }

    /// <summary>
    /// Choose the appropriate colour from the palette given the cell height, expressed as a float value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public new Color ChooseColour(float value)
    {
      if (value != CellPassConsts.NullHeight)
      {
        var index = (int)Math.Floor((value - MinElevation) / ElevationPerBand);
        return Range.InRange(index, 0, ElevationPalette.Length - 1) ? ElevationPalette[index] : UndefinedColor;
      }

      return UndefinedColor;
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
