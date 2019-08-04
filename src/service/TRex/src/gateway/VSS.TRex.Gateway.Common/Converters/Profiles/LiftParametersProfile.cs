using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Models;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class LiftParametersProfile : Profile
  {
    public LiftParametersProfile()
    {
      CreateMap<LiftSettings, ILiftParameters>()
        .ConstructUsing(lp => new LiftParameters())
        .ForMember(x => x.CCVSummarizeTopLayerOnly, opt => opt.MapFrom(o => o.CCVSummarizeTopLayerOnly))
        .ForMember(x => x.MDPSummarizeTopLayerOnly, opt => opt.MapFrom(o => o.MDPSummarizeTopLayerOnly))
        .ForMember(x => x.CCVSummaryTypes, opt => opt.MapFrom(o => (byte)o.CCVSummaryType))
        .ForMember(x => x.MDPSummaryTypes, opt => opt.MapFrom(o => (byte)o.MDPSummaryType))
        .ForMember(x => x.FirstPassThickness, opt => opt.MapFrom(o => o.FirstPassThickness))
        .ForMember(x => x.LiftDetectionType, opt => opt.MapFrom(o => o.LiftDetectionType))
        .ForMember(x => x.LiftThicknessType, opt => opt.MapFrom(o => o.LiftThicknessType))
        .ForMember(x => x.TargetLiftThickness, opt => opt.MapFrom(o => o.LiftThicknessTarget.TargetLiftThickness))
        .ForMember(x => x.AboveToleranceLiftThickness, opt => opt.MapFrom(o => o.LiftThicknessTarget.AboveToleranceLiftThickness))
        .ForMember(x => x.BelowToleranceLiftThickness, opt => opt.MapFrom(o => o.LiftThicknessTarget.BelowToleranceLiftThickness))
        .ForMember(x => x.OverrideMachineThickness, opt => opt.MapFrom(o => o.OverrideMachineThickness))
        .ForMember(x => x.OverridingLiftThickness, opt => opt.MapFrom(o => o.OverridingLiftThickness))
        .ForMember(x => x.IncludeSuperseded, opt => opt.MapFrom(o => o.IncludeSupersededLifts))
        .ForMember(x => x.DeadBandLowerBoundary, opt => opt.MapFrom(o => o.DeadBandLowerBoundary))
        .ForMember(x => x.DeadBandUpperBoundary, opt => opt.MapFrom(o => o.DeadBandUpperBoundary));
    }
  }
}
