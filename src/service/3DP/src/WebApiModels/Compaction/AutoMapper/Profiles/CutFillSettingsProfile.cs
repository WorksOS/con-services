using AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CutFillSettingsProfile : Profile
  {
    public CutFillSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, CutFillSettings>()
        .ForMember(x => x.percents,
          opt => opt.MapFrom(ps => ps.CustomCutFillTolerances));
    }
  }
}
