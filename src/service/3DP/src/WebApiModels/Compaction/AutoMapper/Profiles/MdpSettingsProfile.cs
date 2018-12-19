using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class MdpSettingsProfile : Profile
  {
    public MdpSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, MDPSettings>()
        .ForMember(x => x.OverrideTargetMDP,
          opt => opt.MapFrom(ps => ps.OverrideMachineTargetMdp))
        .ForMember(x => x.MdpTarget,
          opt => opt.MapFrom(ps => ps.CustomTargetMdp))
        .ForMember(x => x.MinMDP,
          opt => opt.MapFrom(ps => ps.MdpMinimum))
        .ForMember(x => x.MaxMDP,
          opt => opt.MapFrom(ps => ps.MdpMaximum))
        .ForMember(x => x.MinMDPPercent,
          opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMinimum))
        .ForMember(x => x.MaxMDPPercent,
          opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMaximum));
    }
  }
}
