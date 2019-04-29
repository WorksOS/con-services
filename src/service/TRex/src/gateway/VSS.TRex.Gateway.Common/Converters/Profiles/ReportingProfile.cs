using System;
using AutoMapper;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class ReportingProfile : Profile
  {
    public ReportingProfile()
    {
      CreateMap<CompactionReportStationOffsetTRexRequest, StationOffsetReportData_ApplicationService>()
        .ForMember(x => x.NumberOfRows,
          opt => opt.Ignore())
        .ForMember(x => x.Rows,
          opt => opt.Ignore());

      CreateMap<CompactionReportStationOffsetTRexRequest, StationOffsetReportRequestArgument_ApplicationService>()
        .ForMember(x => x.ProjectID,
          opt => opt.MapFrom(f => f.ProjectUid))
        .ForMember(x => x.TRexNodeID,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceOffset,
          opt => opt.MapFrom(f => f.CutFillDesignOffset))
        .ForMember(x => x.ReferenceDesignUID,
          opt => opt.MapFrom(f => f.CutFillDesignUid ?? Guid.Empty));

      CreateMap<CompactionReportGridTRexRequest, GriddedReportData>()
        .ForMember(x => x.NumberOfRows,
          opt => opt.Ignore())
        .ForMember(x => x.Rows,
          opt => opt.Ignore());

      CreateMap<CompactionReportGridTRexRequest, GriddedReportRequestArgument>()
        .ForMember(x => x.ProjectID,
          opt => opt.MapFrom(f => f.ProjectUid))
        .ForMember(x => x.TRexNodeID,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceOffset,
          opt => opt.MapFrom(f => f.CutFillDesignOffset))
        .ForMember(x => x.ReferenceDesignUID,
          opt => opt.MapFrom(f => f.CutFillDesignUid ?? Guid.Empty));
    }
  }
}
