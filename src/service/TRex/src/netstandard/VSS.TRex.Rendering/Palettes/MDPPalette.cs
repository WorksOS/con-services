using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class MDPPalette : MDPBasePalette
  {
    private const byte VERSION_NUMBER = 1;

    public bool DisplayTargetMDPColourInPVM { get; set; }
    
    public Color TargetMDPColour = Color.Blue;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Yellow),
      new Transition(20, Color.Red),
      new Transition(40, Color.Aqua),
      new Transition(75, Color.Lime),
      new Transition(100, ColorTranslator.FromHtml("#FF8080"))
    };

    public MDPPalette() : base(Transitions)
    {
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // If we are not using the machine target MDP value then we need to replace the
      // target MDP reported from the machine, with the override value specified in the options
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

      writer.WriteInt(TargetMDPColour.ToArgb());
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
      
      TargetMDPColour = Color.FromArgb(reader.ReadInt());
    }
  }
}
