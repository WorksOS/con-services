using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class OverrideParametersProfile : Profile
  {
    public OverrideParametersProfile()
    {
      const ushort TEMPERATURE_CONVERSION_FACTOR = 10;

      CreateMap<OverridingTargets, IOverrideParameters>()
        //Tell AutoMapper how to make the instance as it's mock for interfaces doesn't handle methods only properties.
        //Also note for interfaces can't have custom resolvers.
        .ConstructUsing(o => new OverrideParameters()) //
        .ForMember(x => x.OverrideMachineCCV, opt => opt.MapFrom(o => o.OverrideTargetCMV))
        .ForMember(x => x.OverridingMachineCCV, opt => opt.MapFrom(o => o.CmvTarget))
        .ForMember(x => x.CMVRange,
          opt => opt.MapFrom(o => new CMVRangePercentageRecord(o.MinCMVPercent, o.MaxCMVPercent)))
        .ForMember(x => x.OverrideMachineMDP, opt => opt.MapFrom(o => o.OverrideTargetMDP))
        .ForMember(x => x.OverridingMachineMDP, opt => opt.MapFrom(o => o.MdpTarget))
        .ForMember(x => x.MDPRange,
          opt => opt.MapFrom(o => new MDPRangePercentageRecord(o.MinMDPPercent, o.MaxMDPPercent)))
        .ForMember(x => x.OverrideTargetPassCount, opt => opt.MapFrom(o => o.OverridingTargetPassCountRange != null))
        .ForMember(x => x.OverridingTargetPassCountRange,
          opt => opt.MapFrom(o =>
            new PassCountRangeRecord(
              o.OverridingTargetPassCountRange == null ? (ushort) 0 : o.OverridingTargetPassCountRange.Min,
              o.OverridingTargetPassCountRange == null ? (ushort) 0 : o.OverridingTargetPassCountRange.Max)))
        .ForMember(x => x.OverrideTemperatureWarningLevels,
          opt => opt.MapFrom(o => o.TemperatureSettings != null && o.TemperatureSettings.OverrideTemperatureRange))
        .ForMember(x => x.OverridingTemperatureWarningLevels,
          opt => opt.MapFrom(o => new TemperatureWarningLevelsRecord(
            o.TemperatureSettings == null ? (ushort) 0 : (ushort) (o.TemperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR),
            o.TemperatureSettings == null ? CellPassConsts.MaxMaterialTempValue : (ushort) (o.TemperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR))))
        .ForMember(x => x.TargetMachineSpeed,
          opt => opt.MapFrom(o => new MachineSpeedExtendedRecord(
            o.MachineSpeedTarget == null ? (ushort) 0 : o.MachineSpeedTarget.MinTargetMachineSpeed,
            o.MachineSpeedTarget == null ? (ushort) 0 : o.MachineSpeedTarget.MaxTargetMachineSpeed)));
    }
  }
}
