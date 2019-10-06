using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CMV summary data
  /// </summary>
  public class CMVSummaryPalette : CMVBasePalette
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The color, which CMV summary data displayed in on a plan view map, where CMV values are greater than target range.
    /// </summary>
    public Color AboveCMVTargetRangeColour = Color.Red;

    /// <summary>
    /// The color, which CMV summary data displayed in on a plan view map, where CMV values are within target range.
    /// </summary>
    public Color WithinCMVTargetRangeColour = Color.Lime;

    /// <summary>
    /// The color, which CMV summary data displayed in on a plan view map, where CMV values are less than target range.
    /// </summary>
    public Color BelowCMVTargetRangeColour = Color.Blue;

    public CMVSummaryPalette() : base(null)
    {
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // If we are not using the machine CMV value then we need to replace the
      // CMV Target reported from the machine, with the override value specified in the options
      if (!UseMachineTargetCMV)
        targetValue = AbsoluteTargetCMV;

      if (targetValue == CellPassConsts.NullCCV)
        return Color.Empty;

      if (value < targetValue * _minTarget)
        return BelowCMVTargetRangeColour;

      if (value > targetValue * _maxTarget)
        return AboveCMVTargetRangeColour;

      return WithinCMVTargetRangeColour;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(AboveCMVTargetRangeColour.ToArgb());
      writer.WriteInt(WithinCMVTargetRangeColour.ToArgb());
      writer.WriteInt(BelowCMVTargetRangeColour.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      AboveCMVTargetRangeColour = Color.FromArgb(reader.ReadInt());
      WithinCMVTargetRangeColour = Color.FromArgb(reader.ReadInt());
      BelowCMVTargetRangeColour = Color.FromArgb(reader.ReadInt());
    }
  }
}
