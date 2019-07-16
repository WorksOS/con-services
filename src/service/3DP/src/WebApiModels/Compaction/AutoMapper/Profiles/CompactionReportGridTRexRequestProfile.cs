using AutoMapper;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CompactionReportGridTRexRequestProfile : Profile
  {
    public CompactionReportGridTRexRequestProfile()
    {
      CreateMap<CompactionReportGridRequest, CompactionReportGridTRexRequest>()
        .ForMember(x => x.ProjectUid, opt => opt.MapFrom(ps => ps.ProjectUid.Value))
        .ForMember(x => x.CutFillDesignUid, 
          opt => opt.MapFrom(ps => 
           (ps.DesignFile != null && ps.DesignFile.FileUid.HasValue) ? ps.DesignFile.FileUid : null))
        .ForMember(x => x.CutFillDesignOffset,
          opt => opt.MapFrom(ps =>
            (ps.DesignFile != null) ? ps.DesignFile.Offset : (double?) null))
        .ForMember(x => x.Overrides, opt => opt.MapFrom(ps => ps.LiftBuildSettings));
    }
  }
}
