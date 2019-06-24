using System;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Profiling.Models;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Common functionality for production data and summary volumes profiles
  /// </summary>
  public class ProfileBaseExecutor : BaseExecutor
  {
    private const ushort MIN_TEMPERATURE = 0;
    private const ushort MAX_TEMPERATURE = 4095;
    private const ushort TEMPERATURE_CONVERSION_FACTOR = 10;

    public ProfileBaseExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    public ProfileBaseExecutor()
    {
    }

    protected OverrideParameters GetOverrideParameters(OverridingTargets overrides)
    {
      var overridingTargetPassCountRange = overrides.OverridingTargetPassCountRange;
      var overrideTargetPassCount = overridingTargetPassCountRange != null;
      var targetPassCountRange = overrideTargetPassCount
        ? new PassCountRangeRecord(overridingTargetPassCountRange.Min, overridingTargetPassCountRange.Max)
        : new PassCountRangeRecord();

      var temperatureSettings = overrides.TemperatureSettings;
      var overrideTemperature = temperatureSettings != null && temperatureSettings.OverrideTemperatureRange;
      var temperatureRange = overrideTemperature ? new TemperatureWarningLevelsRecord(
          Convert.ToUInt16(temperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR),
          Convert.ToUInt16(temperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR)) :
        new TemperatureWarningLevelsRecord(MIN_TEMPERATURE, MAX_TEMPERATURE);

      var targetSpeed = overrides.MachineSpeedTarget;
      var targetMachineSpeed =
        targetSpeed != null ?
          new MachineSpeedExtendedRecord(targetSpeed.MinTargetMachineSpeed, targetSpeed.MaxTargetMachineSpeed) :
          new MachineSpeedExtendedRecord();

      return new OverrideParameters
      {
        CMVRange = new CMVRangePercentageRecord(overrides.MinCMVPercent, overrides.MaxCMVPercent),
        MDPRange = new MDPRangePercentageRecord(overrides.MinMDPPercent, overrides.MaxMDPPercent),
        OverrideMachineCCV = overrides.OverrideTargetCMV,
        OverridingMachineCCV = overrides.CmvTarget,
        OverrideMachineMDP = overrides.OverrideTargetMDP,
        OverridingMachineMDP = overrides.MdpTarget,
        OverrideTargetPassCount = overrideTargetPassCount,
        OverridingTargetPassCountRange = targetPassCountRange,
        OverrideTemperatureWarningLevels = overrideTemperature,
        OverridingTemperatureWarningLevels = temperatureRange,
        TargetMachineSpeed = targetMachineSpeed
      };
    }
  }
}
