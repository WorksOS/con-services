using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class LiftSettingsProfile : Profile
  {
    public LiftSettingsProfile()
    {
      CreateMap<LiftBuildSettings, LiftSettings>()
        .ForMember(x => x.CCVSummarizeTopLayerOnly,
          opt => opt.MapFrom(lbs => lbs.CCVSummarizeTopLayerOnly))
        .ForMember(x => x.MDPSummarizeTopLayerOnly,
          opt => opt.MapFrom(lbs => lbs.MDPSummarizeTopLayerOnly))
        .ForMember(x => x.CCVSummaryType,
          opt => opt.MapFrom(lbs => lbs.CCvSummaryType))
        .ForMember(x => x.MDPSummaryType,
          opt => opt.MapFrom(x => (SummaryType?)null))
        .ForMember(x => x.FirstPassThickness,
          opt => opt.MapFrom(lbs => lbs.FirstPassThickness))
        .ForMember(x => x.LiftDetectionType,
          opt => opt.MapFrom(lbs => lbs.LiftDetectionType))
        .ForMember(x => x.LiftThicknessType,
          opt => opt.MapFrom(lbs => lbs.LiftThicknessType))
        .ForMember(x => x.LiftThicknessTarget,
          opt => opt.MapFrom(lbs => lbs.LiftThicknessTarget))
        .ForMember(x => x.OverrideMachineThickness,
          opt => opt.MapFrom(lbs => lbs.OverridingLiftThickness.HasValue))
        .ForMember(x => x.OverridingLiftThickness,
          opt => opt.MapFrom(lbs => lbs.OverridingLiftThickness ?? 0))
        .ForMember(x => x.IncludeSupersededLifts,
          opt => opt.MapFrom(lbs => lbs.IncludeSupersededLifts ?? false))
        .ForMember(x => x.DeadBandLowerBoundary,
          opt => opt.MapFrom(lbs => lbs.DeadBandLowerBoundary))
        .ForMember(x => x.DeadBandUpperBoundary,
          opt => opt.MapFrom(lbs => lbs.DeadBandUpperBoundary));
    }
  }
}
