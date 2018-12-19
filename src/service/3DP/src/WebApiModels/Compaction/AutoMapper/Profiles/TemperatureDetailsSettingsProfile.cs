using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class TemperatureDetailsSettingsProfile : Profile
  {
    public TemperatureDetailsSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, TemperatureDetailsSettings>()
        .ForMember(x => x.CustomTemperatureDetailsTargets,
          opt => opt.MapFrom(ps => ps.CustomTemperatures));
    }
  }
}
