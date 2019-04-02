using System.Drawing;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw Pass Count summary data
  /// </summary>
  public class PassCountSummaryPalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const ushort PASS_COUNT_DEFAULT_RANGE_MIN = 3;
    private const ushort PASS_COUNT_DEFAULT_RANGE_MAX = 5;

    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are greater than target range.
    /// </summary>
    public Color AbovePassTargetRangeColour = Color.Red;

    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are within target range.
    /// </summary>
    public Color WithinPassTargetRangeColour = Color.YellowGreen;

    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are less than target range.
    /// </summary>
    public Color BelowPassTargetRangeColour = Color.DodgerBlue;

    /// <summary>
    /// The flag is to indicate whether or not the machine Pass Count target range to be user overrides.
    /// </summary>
    public bool UseMachineTargetPass = false;

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    public PassCountRangeRecord TargetPassCountRange = new PassCountRangeRecord(PASS_COUNT_DEFAULT_RANGE_MIN, PASS_COUNT_DEFAULT_RANGE_MAX);

    public PassCountSummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(ushort measuredPassCount, PassCountRangeRecord passTargetRange)
    {
      // If we are not using the machine Pass Target value then we need to replace the
      // Pass Count Target reported from the machine, with the override value specified in the options
      if (!UseMachineTargetPass)
        passTargetRange = TargetPassCountRange;

      if (passTargetRange.Min == CellPassConsts.NullPassCountValue || passTargetRange.Max == CellPassConsts.NullPassCountValue)
        return Color.Empty;

      if (measuredPassCount < passTargetRange.Min)
        return BelowPassTargetRangeColour;

      if (measuredPassCount > passTargetRange.Max)
        return AbovePassTargetRangeColour;

      return WithinPassTargetRangeColour;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(AbovePassTargetRangeColour.ToArgb());
      writer.WriteInt(WithinPassTargetRangeColour.ToArgb());
      writer.WriteInt(BelowPassTargetRangeColour.ToArgb());

      writer.WriteBoolean(UseMachineTargetPass);

      TargetPassCountRange.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      AbovePassTargetRangeColour = Color.FromArgb(reader.ReadInt());
      WithinPassTargetRangeColour = Color.FromArgb(reader.ReadInt());
      BelowPassTargetRangeColour = Color.FromArgb(reader.ReadInt());

      UseMachineTargetPass = reader.ReadBoolean();

      TargetPassCountRange.FromBinary(reader);
    }
  }
}
