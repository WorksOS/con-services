using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Base palette for rendering raw MDP data
  /// </summary>
  public class MDPBasePalette : PaletteBase
  {
    private const byte VERSION_NUMBER = 1;

    private const double MDP_DEFAULT_PERCENTAGE_MIN = 75.0;
    private const double MDP_DEFAULT_PERCENTAGE_MAX = 110.0;

    private const short ABSOLUTE_TARGET_MDP = 50;

    protected double _minTarget => MDPPercentageRange.Min / 100;
    protected double _maxTarget => MDPPercentageRange.Max / 100;

    public MDPBasePalette() { }

    public MDPRangePercentageRecord MDPPercentageRange = new MDPRangePercentageRecord(MDP_DEFAULT_PERCENTAGE_MIN, MDP_DEFAULT_PERCENTAGE_MAX);

    /// <summary>
    /// The flag is to indicate whether or not the machine MDP target to be user overrides.
    /// </summary>
    public bool UseMachineTargetMDP = false;

    /// <summary>
    /// Overriding MDP target value.
    /// </summary>
    public short AbsoluteTargetMDP = ABSOLUTE_TARGET_MDP;

    public MDPBasePalette(Transition[] transitions) : base(transitions)
    {
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(UseMachineTargetMDP);
      writer.WriteShort(AbsoluteTargetMDP);

      MDPPercentageRange.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        UseMachineTargetMDP = reader.ReadBoolean();
        AbsoluteTargetMDP = reader.ReadShort();

        MDPPercentageRange.FromBinary(reader);
      }
    }
  }
}
