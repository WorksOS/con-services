using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// The set of global overriding values when overriding machine targets.
  /// </summary>
  public class OverrideParameters : IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    public bool OverrideMachineCCV;
    public short OverridingMachineCCV;
    public CMVRangePercentageRecord CMVRange;
    public bool OverrideMachineMDP;
    public short OverridingMachineMDP;
    public MDPRangePercentageRecord MDPRange;
    public PassCountRangeRecord OverridingTargetPassCountRange;
    public bool OverrideTargetPassCount;
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;
    public bool OverrideTemperatureWarningLevels;
    public MachineSpeedExtendedRecord TargetMachineSpeed;
    
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
      CMVRange.FromBinary(reader);
      OverrideMachineMDP = reader.ReadBoolean();
      OverridingMachineMDP = reader.ReadShort();
      MDPRange.FromBinary(reader);
      OverrideTargetPassCount = reader.ReadBoolean();
      OverridingTargetPassCountRange.FromBinary(reader);
      OverrideTemperatureWarningLevels = reader.ReadBoolean();
      OverridingTemperatureWarningLevels.FromBinary(reader);
      TargetMachineSpeed.FromBinary(reader);
    }

    //TODO: refactor the BaseRequestArgument to have a base base class with just the below methods to avoid this duplication

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialize this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
