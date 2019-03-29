using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CMV data
  /// </summary>
  public class CMVPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const double CMV_DEFAULT_PERCENTAGE_MIN = 80.0;
    private const double CMV_DEFAULT_PERCENTAGE_MAX = 120.0;

    public bool DisplayTargetCCVColourInPVM { get; set; }
    public bool DisplayDecoupledColourInPVM { get; set; }

    public CMVRangePercentageRecord CMVPercentageRange = new CMVRangePercentageRecord(CMV_DEFAULT_PERCENTAGE_MIN, CMV_DEFAULT_PERCENTAGE_MAX);

    public Color TargetCCVColour = Color.Blue;

    private double _minTarget;
    private double _maxTarget;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Green),
      new Transition(20, Color.Yellow),
      new Transition(40, Color.Olive),
      new Transition(60, Color.Blue),
      new Transition(100, Color.SkyBlue)
    };

    public CMVPalette() : base(Transitions)
    {
      _minTarget = CMVPercentageRange.Min / 100;
      _maxTarget = CMVPercentageRange.Max / 100;
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // Check to see if the value is in the target range and use the target CMV colour
      // if it is. CCVRange holds a min/max percentage of target CMV...
      if (DisplayTargetCCVColourInPVM && (value >= targetValue * _minTarget && value <= targetValue * _maxTarget))
        return TargetCCVColour;

      return ChooseColour(value);
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(DisplayDecoupledColourInPVM);
      writer.WriteBoolean(DisplayTargetCCVColourInPVM);

      CMVPercentageRange.ToBinary(writer);

      writer.WriteInt(TargetCCVColour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      DisplayDecoupledColourInPVM = reader.ReadBoolean();
      DisplayTargetCCVColourInPVM = reader.ReadBoolean();

      CMVPercentageRange.FromBinary(reader);

      TargetCCVColour = Color.FromArgb(reader.ReadInt());
    }
  }
}
