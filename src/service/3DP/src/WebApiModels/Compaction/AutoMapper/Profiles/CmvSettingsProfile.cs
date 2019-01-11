using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CmvSettingsProfile : Profile
  {
    public CmvSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, CMVSettings>()
        .ForMember(x => x.OverrideTargetCMV,
          opt => opt.MapFrom(ps => ps.OverrideMachineTargetCmv))
        .ForMember(x => x.CmvTarget,
          opt => opt.MapFrom(ps => ps.CustomTargetCmv))
        .ForMember(x => x.MinCMV,
          opt => opt.MapFrom(ps => ps.CmvMinimum))
        .ForMember(x => x.MaxCMV,
          opt => opt.MapFrom(ps => ps.CmvMaximum))
        .ForMember(x => x.MinCMVPercent,
          opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMinimum))
        .ForMember(x => x.MaxCMVPercent,
          opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMaximum));
    }
  }
}
