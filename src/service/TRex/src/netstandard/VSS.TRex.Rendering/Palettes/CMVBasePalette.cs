using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Base palette for rendering raw CMV data.
  /// </summary>
  public class CMVBasePalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const double CMV_DEFAULT_PERCENTAGE_MIN = 80.0;
    private const double CMV_DEFAULT_PERCENTAGE_MAX = 120.0;

    private const short ABSOLUTE_TARGET_CMV = 70;

    protected double _minTarget => CMVPercentageRange.Min / 100;
    protected double _maxTarget => CMVPercentageRange.Max / 100;

    public CMVRangePercentageRecord CMVPercentageRange = new CMVRangePercentageRecord(CMV_DEFAULT_PERCENTAGE_MIN, CMV_DEFAULT_PERCENTAGE_MAX);

    /// <summary>
    /// The flag is to indicate whether or not the machine CMV target to be user overrides.
    /// </summary>
    public bool UseMachineTargetCMV = false;

    /// <summary>
    /// Default overriding CMV target value.
    /// </summary>
    public short AbsoluteTargetCMV = ABSOLUTE_TARGET_CMV;

    public CMVBasePalette(Transition[] transitions) : base(transitions)
    {
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(UseMachineTargetCMV);
      writer.WriteShort(AbsoluteTargetCMV);

      CMVPercentageRange.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      UseMachineTargetCMV = reader.ReadBoolean();
      AbsoluteTargetCMV = reader.ReadShort();

      CMVPercentageRange.FromBinary(reader);
    }
  }
}
