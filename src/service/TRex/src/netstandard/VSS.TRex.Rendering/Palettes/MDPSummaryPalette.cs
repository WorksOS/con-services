using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class MDPSummaryPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const double MDP_DEFAULT_PERCENTAGE_MIN = 75.0;
    private const double MDP_DEFAULT_PERCENTAGE_MAX = 110.0;

    private const short ABSOLUTE_TARGET_MDP = 50;

    public bool DisplayTargetMDPColourInPVM { get; set; }

    public MDPRangePercentageRecord MDPPercentageRange = new MDPRangePercentageRecord(MDP_DEFAULT_PERCENTAGE_MIN, MDP_DEFAULT_PERCENTAGE_MAX);

    public Color TargetMDPColour = Color.Blue;

    /// <summary>
    /// The flag is to indicate whether or not the machine MDP target to be user overrides.
    /// </summary>
    public bool UseMachineTargetMDP = false;

    /// <summary>
    /// Default overriding MDP target value.
    /// </summary>
    public short AbsoluteTargetMDP = ABSOLUTE_TARGET_MDP;

    private double _minTarget;
    private double _maxTarget;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Yellow),
      new Transition(1000, Color.Red),
      new Transition(1200, Color.Aqua),
      new Transition(1400, Color.Lime),
      new Transition(1600, ColorTranslator.FromHtml("#FF8080"))
    };

    public MDPSummaryPalette() : base(Transitions)
    {
      _minTarget = MDPPercentageRange.Min / 100;
      _maxTarget = MDPPercentageRange.Max / 100;
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // If we are not using the machine target MDP value then we need to replace the
      // target MDP report from the machine, with the override value specified in the options
      if (!UseMachineTargetMDP)
        targetValue = AbsoluteTargetMDP;

      // Check to see if the value is in the target range and use the target MDP colour
      // if it is. MDPRange holds a min/max percentage of target MDP...
      return DisplayTargetMDPColourInPVM && value >= targetValue * _minTarget && value <= targetValue * _maxTarget
        ? TargetMDPColour
        : ChooseColour(value);
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(DisplayTargetMDPColourInPVM);

      MDPPercentageRange.ToBinary(writer);

      writer.WriteInt(TargetMDPColour.ToArgb());
      writer.WriteBoolean(UseMachineTargetMDP);
      writer.WriteShort(AbsoluteTargetMDP);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      DisplayTargetMDPColourInPVM = reader.ReadBoolean();

      MDPPercentageRange.FromBinary(reader);
      
      TargetMDPColour = Color.FromArgb(reader.ReadInt());
      UseMachineTargetMDP = reader.ReadBoolean();
      AbsoluteTargetMDP = reader.ReadShort();
    }
  }
}
