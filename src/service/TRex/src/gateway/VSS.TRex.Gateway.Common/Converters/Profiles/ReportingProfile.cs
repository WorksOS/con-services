using System;
using AutoMapper;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
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
        .ForMember(x => x.OriginatingIgniteNodeId,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceDesign,
          opt => opt.MapFrom<CustomStationOffsetReferenceDesignResolver>())
        .ForMember(x => x.Overrides, opt => opt.MapFrom(o => o.Overrides))
        .ForMember(x => x.LiftParams, opt => opt.MapFrom(o => o.LiftSettings));

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
        .ForMember(x => x.OriginatingIgniteNodeId,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceDesign,
          opt => opt.MapFrom<CustomGridReferenceDesignResolver>())
        .ForMember(x => x.Overrides, opt => opt.MapFrom(o => o.Overrides))
        .ForMember(x => x.LiftParams, opt => opt.MapFrom(o => o.LiftSettings));
    }


    public class CustomGridReferenceDesignResolver : IValueResolver<CompactionReportTRexRequest, GriddedReportRequestArgument, DesignOffset>
    {
      public DesignOffset Resolve(CompactionReportTRexRequest src, GriddedReportRequestArgument dst, DesignOffset member, ResolutionContext context)
      {
        return new DesignOffset(src.CutFillDesignUid ?? Guid.Empty, src.CutFillDesignOffset ?? 0);
      }
    }

    public class CustomStationOffsetReferenceDesignResolver : IValueResolver<CompactionReportStationOffsetTRexRequest, StationOffsetReportRequestArgument_ApplicationService, DesignOffset>
    {
      public DesignOffset Resolve(CompactionReportStationOffsetTRexRequest src, StationOffsetReportRequestArgument_ApplicationService dst, DesignOffset member, ResolutionContext context)
      {
        return new DesignOffset(src.CutFillDesignUid ?? Guid.Empty, src.CutFillDesignOffset ?? 0);
      }
    }
  }
}
