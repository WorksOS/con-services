using AutoMapper;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CompactionReportStationOffsetTRexRequestProfile : Profile
  {
    public CompactionReportStationOffsetTRexRequestProfile()
    {
      CreateMap<CompactionReportStationOffsetRequest, CompactionReportStationOffsetTRexRequest>()
        .ForMember(x => x.ProjectUid, opt => opt.MapFrom(ps => ps.ProjectUid.Value))
        .ForMember(x => x.CutFillDesignUid, opt => opt.MapFrom(ps =>
            (ps.DesignFile != null && ps.DesignFile.FileUid.HasValue) ? ps.DesignFile.FileUid : null))
        .ForMember(x => x.CutFillDesignOffset, opt => opt.MapFrom(ps =>
            (ps.DesignFile != null) ? ps.DesignFile.Offset : (double?)null))
        .ForMember(x => x.AlignmentDesignUid, opt => opt.MapFrom(ps =>
            (ps.AlignmentFile != null && ps.AlignmentFile.FileUid.HasValue) ? ps.AlignmentFile.FileUid : null))
        ;
    }
  }
}
