using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Common.Models
{
  /// <summary>
  /// The set of global overriding values when overriding machine targets.
  /// </summary>
  public class OverrideParameters : VersionCheckedBinarizableSerializationBase, IOverrideParameters
  {
    private const byte VERSION_NUMBER = 1;

    //The private fields are necessary for serialization to work with the structs
    public bool OverrideMachineCCV { get; set; }
    public short OverridingMachineCCV { get; set; }
    private CMVRangePercentageRecord _CMVRange;
    public CMVRangePercentageRecord CMVRange
    {
      get => _CMVRange;
      set => _CMVRange = value; 
    }
    public bool OverrideMachineMDP { get; set; }
    public short OverridingMachineMDP { get; set; }
    private MDPRangePercentageRecord _MDPRange;
    public MDPRangePercentageRecord MDPRange
    {
      get => _MDPRange;
      set => _MDPRange = value; 
    }
    private PassCountRangeRecord _OverridingTargetPassCountRange;
    public PassCountRangeRecord OverridingTargetPassCountRange
    {
      get => _OverridingTargetPassCountRange;
      set => _OverridingTargetPassCountRange = value;
    }
    public bool OverrideTargetPassCount { get; set; }
    private TemperatureWarningLevelsRecord _OverridingTemperatureWarningLevels;
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels
    {
      get => _OverridingTemperatureWarningLevels;
      set => _OverridingTemperatureWarningLevels = value;
    }
    public bool OverrideTemperatureWarningLevels { get; set; }
    private MachineSpeedExtendedRecord _TargetMachineSpeed;
    public MachineSpeedExtendedRecord TargetMachineSpeed
    {
      get => _TargetMachineSpeed;
      set => _TargetMachineSpeed = value;
    }

    public OverrideParameters()
    {
      //Set defaults
      OverrideMachineCCV = false;
      OverridingMachineCCV = 0;
      CMVRange = new CMVRangePercentageRecord();
      OverrideMachineMDP = false;
      OverridingMachineMDP = 0;
      MDPRange = new MDPRangePercentageRecord();
      OverridingTargetPassCountRange = new PassCountRangeRecord();
      OverrideTargetPassCount = false;
      OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord();
      OverrideTemperatureWarningLevels = false;
      TargetMachineSpeed = new MachineSpeedExtendedRecord();
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(OverrideMachineCCV);
      writer.WriteShort(OverridingMachineCCV);
      _CMVRange.ToBinary(writer);
      writer.WriteBoolean(OverrideMachineMDP);
      writer.WriteShort(OverridingMachineMDP);
      _MDPRange.ToBinary(writer);
      writer.WriteBoolean(OverrideTargetPassCount);
      _OverridingTargetPassCountRange.ToBinary(writer);
      writer.WriteBoolean(OverrideTemperatureWarningLevels);
      _OverridingTemperatureWarningLevels.ToBinary(writer);
      _TargetMachineSpeed.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        OverrideMachineCCV = reader.ReadBoolean();
        OverridingMachineCCV = reader.ReadShort();
        _CMVRange.FromBinary(reader);
        OverrideMachineMDP = reader.ReadBoolean();
        OverridingMachineMDP = reader.ReadShort();
        _MDPRange.FromBinary(reader);
        OverrideTargetPassCount = reader.ReadBoolean();
        _OverridingTargetPassCountRange.FromBinary(reader);
        OverrideTemperatureWarningLevels = reader.ReadBoolean();
        _OverridingTemperatureWarningLevels.FromBinary(reader);
        _TargetMachineSpeed.FromBinary(reader);
      }
    }
  }
}
