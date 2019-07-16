using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class OverridingTargetsProfile : Profile
  {
    public OverridingTargetsProfile()
    {
      CreateMap<LiftBuildSettings, OverridingTargets>()
        .ForMember(x => x.OverrideTargetCMV,
          opt => opt.MapFrom(lbs => lbs.OverridingMachineCCV.HasValue))
        .ForMember(x => x.CmvTarget,
          opt => opt.MapFrom(lbs => lbs.OverridingMachineCCV ?? 0))
        .ForMember(x => x.MinCMVPercent,
          opt => opt.MapFrom(lbs => lbs.CCVRange.Min))
        .ForMember(x => x.MaxCMVPercent,
          opt => opt.MapFrom(lbs => lbs.CCVRange.Max))
        .ForMember(x => x.OverrideTargetMDP,
          opt => opt.MapFrom(lbs => lbs.OverridingMachineMDP.HasValue))
        .ForMember(x => x.MdpTarget,
          opt => opt.MapFrom(lbs => lbs.OverridingMachineMDP ?? 0))
        .ForMember(x => x.MinMDPPercent,
          opt => opt.MapFrom(lbs => lbs.MDPRange.Min))
        .ForMember(x => x.MaxMDPPercent,
          opt => opt.MapFrom(lbs => lbs.MDPRange.Max))
        .ForMember(x => x.OverridingTargetPassCountRange,
          opt => opt.MapFrom(lbs => lbs.OverridingTargetPassCountRange))
        .ForMember(x => x.TemperatureSettings,
          opt => opt.MapFrom<AutoMapperUtility.CustomTemperatureSettingsResolver>())
        .ForMember(x => x.MachineSpeedTarget,
          opt => opt.MapFrom(lbs => lbs.MachineSpeedTarget));
    }
  }
}
