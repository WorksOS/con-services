using System;
using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class OverrideParametersProfile : Profile
  {
    public OverrideParametersProfile()
    {
      CreateMap<OverridingTargets, OverrideParameters>()
        .ForMember(x => x.OverrideMachineCCV, opt => opt.MapFrom(o => o.OverrideTargetCMV))
        .ForMember(x => x.OverridingMachineCCV, opt => opt.MapFrom(o => o.CmvTarget))
        .ForMember(x => x.CMVRange, opt => opt.ResolveUsing<CustomCMVRangeResolver>())
        .ForMember(x => x.OverrideMachineMDP, opt => opt.MapFrom(o => o.OverrideTargetMDP))
        .ForMember(x => x.OverridingMachineMDP, opt => opt.MapFrom(o => o.MdpTarget))
        .ForMember(x => x.MDPRange, opt => opt.ResolveUsing<CustomMDPRangeResolver>())
        .ForMember(x => x.OverrideTargetPassCount, opt => opt.MapFrom(o => o.OverridingTargetPassCountRange != null))
        .ForMember(x => x.OverridingTargetPassCountRange, opt => opt.ResolveUsing<CustomPassCountRangeResolver>())
        .ForMember(x => x.OverrideTemperatureWarningLevels, 
          opt => opt.MapFrom(o => o.TemperatureSettings != null && o.TemperatureSettings.OverrideTemperatureRange))
        .ForMember(x => x.OverridingTemperatureWarningLevels, opt => opt.ResolveUsing<CustomTemperatureRangeResolver>())
        .ForMember(x => x.TargetMachineSpeed, opt => opt.ResolveUsing<CustomSpeedRangeResolver>());
    }

    public class CustomCMVRangeResolver : IValueResolver<OverridingTargets, OverrideParameters, CMVRangePercentageRecord>
    {
      public CMVRangePercentageRecord Resolve(OverridingTargets src, OverrideParameters dst, CMVRangePercentageRecord member, ResolutionContext context)
      {
        return new CMVRangePercentageRecord(src.MinCMVPercent, src.MaxCMVPercent);
      }
    }


    public class CustomMDPRangeResolver : IValueResolver<OverridingTargets, OverrideParameters, MDPRangePercentageRecord>
    {
      public MDPRangePercentageRecord Resolve(OverridingTargets src, OverrideParameters dst, MDPRangePercentageRecord member, ResolutionContext context)
      {
        return new MDPRangePercentageRecord(src.MinMDPPercent, src.MaxMDPPercent);
      }
    }
    public class CustomPassCountRangeResolver : IValueResolver<OverridingTargets, OverrideParameters, PassCountRangeRecord>
    {
      public PassCountRangeRecord Resolve(OverridingTargets src, OverrideParameters dst, PassCountRangeRecord member, ResolutionContext context)
      {
        var overridingTargetPassCountRange = src.OverridingTargetPassCountRange;
        var overrideTargetPassCount = overridingTargetPassCountRange != null;
        var targetPassCountRange = overrideTargetPassCount
          ? new PassCountRangeRecord(overridingTargetPassCountRange.Min, overridingTargetPassCountRange.Max)
          : new PassCountRangeRecord();
        return targetPassCountRange;
      }
    }
    public class CustomTemperatureRangeResolver : IValueResolver<OverridingTargets, OverrideParameters, TemperatureWarningLevelsRecord>
    {
      public TemperatureWarningLevelsRecord Resolve(OverridingTargets src, OverrideParameters dst, TemperatureWarningLevelsRecord member, ResolutionContext context)
      {
        const ushort TEMPERATURE_CONVERSION_FACTOR = 10;

        var temperatureSettings = src.TemperatureSettings;
        var overrideTemperature = temperatureSettings != null && temperatureSettings.OverrideTemperatureRange;
        var temperatureRange = overrideTemperature ? new TemperatureWarningLevelsRecord(
            Convert.ToUInt16(temperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR),
            Convert.ToUInt16(temperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR)) :
          new TemperatureWarningLevelsRecord(0, CellPassConsts.MaxMaterialTempValue);
        return temperatureRange;
      }
    }
    public class CustomSpeedRangeResolver : IValueResolver<OverridingTargets, OverrideParameters, MachineSpeedExtendedRecord>
    {
      public MachineSpeedExtendedRecord Resolve(OverridingTargets src, OverrideParameters dst, MachineSpeedExtendedRecord member, ResolutionContext context)
      {
        var targetSpeed = src.MachineSpeedTarget;
        var targetMachineSpeed =
          targetSpeed != null ?
            new MachineSpeedExtendedRecord(targetSpeed.MinTargetMachineSpeed, targetSpeed.MaxTargetMachineSpeed) :
            new MachineSpeedExtendedRecord();
        return targetMachineSpeed;
      }
    }

  }
}
