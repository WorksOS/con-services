using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// The set of global overriding values when overriding machine targets.
  /// </summary>
  public class OverrideParameters
  {
    private const byte VERSION_NUMBER = 1;

    public bool OverrideMachineCCV { get; set; }
    public short OverridingMachineCCV { get; set; }
    public CMVRangePercentageRecord CMVRange { get; set; }
    public bool OverrideMachineMDP { get; set; }
    public short OverridingMachineMDP { get; set; }
    public MDPRangePercentageRecord MDPRange { get; set; }
    public PassCountRangeRecord OverridingTargetPassCountRange { get; set; }
    public bool OverrideTargetPassCount { get; set; }
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels { get; set; }
    public bool OverrideTemperatureWarningLevels { get; set; }
    public MachineSpeedExtendedRecord TargetMachineSpeed { get; set; }
    
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
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(OverrideMachineCCV);
      writer.WriteShort(OverridingMachineCCV);
      CMVRange.ToBinary(writer);
      writer.WriteBoolean(OverrideMachineMDP);
      writer.WriteShort(OverridingMachineMDP);
      MDPRange.ToBinary(writer);
      writer.WriteBoolean(OverrideTargetPassCount);
      OverridingTargetPassCountRange.ToBinary(writer);
      writer.WriteBoolean(OverrideTemperatureWarningLevels);
      OverridingTemperatureWarningLevels.ToBinary(writer);
      TargetMachineSpeed.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
      OverrideMachineCCV = reader.ReadBoolean();
      OverridingMachineCCV = reader.ReadShort();
      CMVRange = new CMVRangePercentageRecord();
      CMVRange.FromBinary(reader);
      OverrideMachineMDP = reader.ReadBoolean();
      OverridingMachineMDP = reader.ReadShort();
      MDPRange = new MDPRangePercentageRecord();
      MDPRange.FromBinary(reader);
      OverrideTargetPassCount = reader.ReadBoolean();
      OverridingTargetPassCountRange = new PassCountRangeRecord();
      OverridingTargetPassCountRange.FromBinary(reader);
      OverrideTemperatureWarningLevels = reader.ReadBoolean();
      OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord();
      OverridingTemperatureWarningLevels.FromBinary(reader);
      TargetMachineSpeed = new MachineSpeedExtendedRecord();
      TargetMachineSpeed.FromBinary(reader);
    }
  }
}
