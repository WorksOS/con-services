using AutoMapper;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Models;
using VSS.TRex.Types.Types;

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
        .ForMember(x => x.CCVSummaryTypes, opt => opt.MapFrom(o => ResolveCCVSummaryType(o.CCVSummaryType)))
        .ForMember(x => x.MDPSummaryTypes, opt => opt.MapFrom(o => ResolveMDPSummaryType(o.MDPSummaryType)))
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

    private CCVSummaryTypes ResolveCCVSummaryType(SummaryType? summaryType)
    {
      if (summaryType.HasValue)
      {
        switch (summaryType.Value)
        {
          case SummaryType.WorkInProgress:
            return CCVSummaryTypes.WorkInProgress;
          case SummaryType.Thickness:
            return CCVSummaryTypes.Thickness;
          default:
            return CCVSummaryTypes.Compaction;
        }
      }
      return CCVSummaryTypes.None;
    }

    private MDPSummaryTypes ResolveMDPSummaryType(SummaryType? summaryType)
    {
      if (summaryType.HasValue)
      {
        switch (summaryType.Value)
        {
          case SummaryType.WorkInProgress:
            return MDPSummaryTypes.WorkInProgress;
          case SummaryType.Thickness:
            return MDPSummaryTypes.Thickness;
          default:
            return MDPSummaryTypes.Compaction;
        }
      }
      return MDPSummaryTypes.None;
    }

  }
}
