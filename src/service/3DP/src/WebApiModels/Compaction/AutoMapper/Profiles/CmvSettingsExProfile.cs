using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CmvSettingsExProfile : Profile
  {
    public CmvSettingsExProfile()
    {
      CreateMap<CompactionProjectSettings, CMVSettingsEx>()
        .IncludeBase<CompactionProjectSettings, CMVSettings>()
        .ForMember(x => x.CustomCMVDetailTargets,
          opt => opt.MapFrom(ps => ps.CustomCMVs));
    }
  }
}
