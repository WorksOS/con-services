using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Types;

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

    private const short ABSOLUTE_TARGET_MDP = 1200;

    protected double _minTarget;
    protected double _maxTarget;
    
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
      _minTarget = MDPPercentageRange.Min / 100;
      _maxTarget = MDPPercentageRange.Max / 100;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(UseMachineTargetMDP);
      writer.WriteShort(AbsoluteTargetMDP);

      MDPPercentageRange.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      UseMachineTargetMDP = reader.ReadBoolean();
      AbsoluteTargetMDP = reader.ReadShort();

      MDPPercentageRange.FromBinary(reader);
    }
  }
}
