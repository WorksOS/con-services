using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class TemperatureSettingsProfile : Profile
  {
    public TemperatureSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, TemperatureSettings>()
        .ForMember(x => x.OverrideTemperatureRange,
          opt => opt.MapFrom(ps => ps.OverrideMachineTargetTemperature))
        .ForMember(x => x.MinTemperature,
          opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMinimum))
        .ForMember(x => x.MaxTemperature,
          opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMaximum));
    }
  }
}
