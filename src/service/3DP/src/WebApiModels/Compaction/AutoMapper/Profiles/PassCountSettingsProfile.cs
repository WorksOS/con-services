using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class PassCountSettingsProfile : Profile
  {
    public PassCountSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, PassCountSettings>()
        .ForMember(x => x.passCounts,
          opt => opt.MapFrom(ps => ps.CustomPassCounts));
    }
  }
}
