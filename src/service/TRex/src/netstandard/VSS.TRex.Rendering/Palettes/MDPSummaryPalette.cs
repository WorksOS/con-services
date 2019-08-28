using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP summary data
  /// </summary>
  public class MDPSummaryPalette : MDPBasePalette
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The color, which MDP summary data displayed in on a plan view map, where MDP values are greater than target range.
    /// </summary>
    public Color AboveMDPTargetRangeColour = Color.Red;

    /// <summary>
    /// The color, which MDP summary data displayed in on a plan view map, where MDP values are within target range.
    /// </summary>
    public Color WithinMDPTargetRangeColour = Color.Lime;

    /// <summary>
    /// The color, which MDP summary data displayed in on a plan view map, where MDP values are less than target range.
    /// </summary>
    public Color BelowMDPTargetRangeColour = Color.Blue;

    public MDPSummaryPalette() : base(null)
    {
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // If we are not using the machine CMV value then we need to replace the
      // CMV Target reported from the machine, with the override value specified in the options
      if (!UseMachineTargetMDP)
        targetValue = AbsoluteTargetMDP;

      if (targetValue == CellPassConsts.NullCCV)
        return Color.Empty;

      if (value < targetValue * _minTarget)
        return BelowMDPTargetRangeColour;

      if (value > targetValue * _maxTarget)
        return AboveMDPTargetRangeColour;

      return WithinMDPTargetRangeColour;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(AboveMDPTargetRangeColour.ToArgb());
      writer.WriteInt(WithinMDPTargetRangeColour.ToArgb());
      writer.WriteInt(BelowMDPTargetRangeColour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      AboveMDPTargetRangeColour = Color.FromArgb(reader.ReadInt());
      WithinMDPTargetRangeColour = Color.FromArgb(reader.ReadInt());
      BelowMDPTargetRangeColour = Color.FromArgb(reader.ReadInt());
    }
  }
}
