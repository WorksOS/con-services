using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class LiftBuildSettingsProfile : Profile
  {
    public LiftBuildSettingsProfile()
    {
      CreateMap<CompactionProjectSettings, LiftBuildSettings>()
        .ForMember(x => x.CCVRange,
          opt => opt.MapFrom<AutoMapperUtility.CustomCCVRangePercentageResolver>())
        .ForMember(x => x.CCVSummarizeTopLayerOnly,
          opt => opt.Ignore())//Raptor only uses this when using lifts (all layers)
        .ForMember(x => x.CCvSummaryType,
          opt => opt.Ignore())
        .ForMember(x => x.DeadBandLowerBoundary,
          opt => opt.Ignore())
        .ForMember(x => x.DeadBandUpperBoundary,
          opt => opt.Ignore())
        .ForMember(x => x.FirstPassThickness,
          opt => opt.Ignore())
        .ForMember(x => x.LiftDetectionType,
          opt => opt.MapFrom(x => LiftDetectionType.None))
        .ForMember(x => x.LiftThicknessType,
          opt => opt.MapFrom(x => LiftThicknessType.Compacted))
        .ForMember(x => x.MDPRange,
          opt => opt.MapFrom<AutoMapperUtility.CustomMDPRangePercentageResolver>())
        .ForMember(x => x.MDPSummarizeTopLayerOnly,
          opt => opt.Ignore())//Raptor only uses this when using lifts (all layers)
        .ForMember(x => x.OverridingLiftThickness,
          opt => opt.Ignore())
        .ForMember(x => x.OverridingMachineCCV,
          opt => opt.MapFrom(ps => ps.NullableCustomTargetCmv))
        .ForMember(x => x.OverridingMachineMDP,
          opt => opt.MapFrom(ps => ps.NullableCustomTargetMdp))
        .ForMember(x => x.OverridingTargetPassCountRange,
          opt => opt.MapFrom<AutoMapperUtility.CustomTargetPassCountRangeResolver>())
        .ForMember(x => x.OverridingTemperatureWarningLevels,
          opt => opt.MapFrom<AutoMapperUtility.CustomTemperatureWarningLevelsResolver>())
        .ForMember(x => x.IncludeSupersededLifts,
          opt => opt.Ignore())//Raptor only uses this when using lifts (all layers). For 'no lift' is always true.
        .ForMember(x => x.LiftThicknessTarget,
          opt => opt.Ignore())
        .ForMember(x => x.MachineSpeedTarget,
          opt => opt.MapFrom<AutoMapperUtility.CustomMachineSpeedTargetResolver>());
    }
  }
}
