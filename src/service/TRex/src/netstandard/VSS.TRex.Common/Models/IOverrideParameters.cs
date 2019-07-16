using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Common.Models
{
  public interface IOverrideParameters : IFromToBinary
  {
    bool OverrideMachineCCV { get; set; }
    short OverridingMachineCCV { get; set; }
    CMVRangePercentageRecord CMVRange { get; set; }
    bool OverrideMachineMDP { get; set; }
    short OverridingMachineMDP { get; set; }
    MDPRangePercentageRecord MDPRange { get; set; }
    PassCountRangeRecord OverridingTargetPassCountRange { get; set; }
    bool OverrideTargetPassCount { get; set; }
    TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels { get; set; }
    bool OverrideTemperatureWarningLevels { get; set; }
    MachineSpeedExtendedRecord TargetMachineSpeed { get; set; }
  }
}
