using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class AutoMapperUtility
  {
    private static MapperConfiguration _automapperConfiguration;

    public static MapperConfiguration AutomapperConfiguration
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapperConfiguration;
      }
    }

    private static IMapper _automapper;

    public static IMapper Automapper
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapper;
      }
    }


    public static void ConfigureAutomapper()
    {

      _automapperConfiguration = new MapperConfiguration(
        //define mappings <source type, destination type>
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.AddProfile<CmvSettingsProfile>();
          cfg.AddProfile<CmvSettingsExProfile>(); 
          cfg.AddProfile<MdpSettingsProfile>();
          cfg.AddProfile<TemperatureSettingsProfile>();
          cfg.AddProfile<TemperatureDetailsSettingsProfile>();
          cfg.AddProfile<PassCountSettingsProfile>();
          cfg.AddProfile<CmvPercentChangeSettingsProfile>();
          cfg.AddProfile<CutFillSettingsProfile>();
          cfg.AddProfile<LiftBuildSettingsProfile>();
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
     
    }

  
    public class CustomCCVRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, CCVRangePercentage>
    {
      public CCVRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, CCVRangePercentage member, ResolutionContext context)
      {
        return src.OverrideDefaultTargetRangeCmvPercent ? 
          new CCVRangePercentage(src.CustomTargetCmvPercentMinimum, src.CustomTargetCmvPercentMaximum) :
          new CCVRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMaximum);
      }
    }

    public class CustomMDPRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MDPRangePercentage>
    {
      public MDPRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MDPRangePercentage member, ResolutionContext context)
      {
        return src.OverrideDefaultTargetRangeMdpPercent ? 
          new MDPRangePercentage(src.CustomTargetMdpPercentMinimum, src.CustomTargetMdpPercentMaximum) :
          new MDPRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMaximum);
      }
    }

    public class CustomTargetPassCountRangeResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TargetPassCountRange>
    {
      public TargetPassCountRange Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TargetPassCountRange member, ResolutionContext context)
      {
        return src.OverrideMachineTargetPassCount
          ? new TargetPassCountRange((ushort) src.customTargetPassCountMinimum, (ushort) src.customTargetPassCountMaximum)
          : null;
      }
    }
    public class CustomTemperatureWarningLevelsResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TemperatureWarningLevels>
    {
      public TemperatureWarningLevels Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TemperatureWarningLevels member, ResolutionContext context)
      {
        return src.OverrideMachineTargetTemperature
          ? new TemperatureWarningLevels(src.CustomTargetTemperatureWarningLevelMinimum, src.CustomTargetTemperatureWarningLevelMaximum) : null;
      }
    }

    public class CustomMachineSpeedTargetResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MachineSpeedTarget>
    {
      public MachineSpeedTarget Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MachineSpeedTarget member, ResolutionContext context)
      {
        return new MachineSpeedTarget(src.CustomTargetSpeedMinimum, src.CustomTargetSpeedMaximum);
      }
    }

    public class CmvSettingsProfile : Profile
    {
      public CmvSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, CMVSettings>()
          .ForMember(x => x.OverrideTargetCMV,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetCmv))
          .ForMember(x => x.CmvTarget,
            opt => opt.MapFrom(ps => ps.CustomTargetCmv))
          .ForMember(x => x.MinCMV,
            opt => opt.MapFrom(ps => ps.CmvMinimum))
          .ForMember(x => x.MaxCMV,
            opt => opt.MapFrom(ps => ps.CmvMaximum))
          .ForMember(x => x.MinCMVPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMinimum))
          .ForMember(x => x.MaxCMVPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMaximum));
      }
    }

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

    public class MdpSettingsProfile : Profile
    {
      public MdpSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, MDPSettings>()
          .ForMember(x => x.OverrideTargetMDP,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetMdp))
          .ForMember(x => x.MdpTarget,
            opt => opt.MapFrom(ps => ps.CustomTargetMdp))
          .ForMember(x => x.MinMDP,
            opt => opt.MapFrom(ps => ps.MdpMinimum))
          .ForMember(x => x.MaxMDP,
            opt => opt.MapFrom(ps => ps.MdpMaximum))
          .ForMember(x => x.MinMDPPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMinimum))
          .ForMember(x => x.MaxMDPPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMaximum));
      }
    }

    public class TemperatureSettingsProfile : Profile
    {
      public TemperatureSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, TemperatureSettings>()
          .ForMember(x => x.OverrideTemperatureRange,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetTemperature))
          .ForMember(x => x.MinTemperature,
            opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMinimum))
          .ForMember(x => x.MaxTemperature,
            opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMaximum));
      }
    }

    public class TemperatureDetailsSettingsProfile : Profile
    {
      public TemperatureDetailsSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, TemperatureDetailsSettings>()
          .ForMember(x => x.CustomTemperatureDetailsTargets,
            opt => opt.MapFrom(ps => ps.CustomTemperatures));
      }
    }

    public class PassCountSettingsProfile : Profile
    {
      public PassCountSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, PassCountSettings>()
          .ForMember(x => x.passCounts,
            opt => opt.MapFrom(ps => ps.CustomPassCounts));
      }
    }

    public class CmvPercentChangeSettingsProfile : Profile
    {
     public CmvPercentChangeSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, CmvPercentChangeSettings>()
          .ForMember(x => x.percents,
            opt => opt.MapFrom(ps => ps.CmvPercentChange));
      }
    }

    public class CutFillSettingsProfile : Profile
    {
      public CutFillSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, CutFillSettings>()
          .ForMember(x => x.percents,
            opt => opt.MapFrom(ps => ps.CustomCutFillTolerances));
      }
    }

    public class LiftBuildSettingsProfile : Profile
    {
      public LiftBuildSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, LiftBuildSettings>()
          .ForMember(x => x.CCVRange,
            opt => opt.ResolveUsing<CustomCCVRangePercentageResolver>())
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
            opt => opt.UseValue(LiftDetectionType.None))
          .ForMember(x => x.LiftThicknessType,
            opt => opt.UseValue(LiftThicknessType.Compacted))
          .ForMember(x => x.MDPRange,
            opt => opt.ResolveUsing<CustomMDPRangePercentageResolver>())
          .ForMember(x => x.MDPSummarizeTopLayerOnly,
            opt => opt.Ignore())//Raptor only uses this when using lifts (all layers)
          .ForMember(x => x.OverridingLiftThickness,
            opt => opt.Ignore())
          .ForMember(x => x.OverridingMachineCCV,
            opt => opt.MapFrom(ps => ps.NullableCustomTargetCmv))
          .ForMember(x => x.OverridingMachineMDP,
            opt => opt.MapFrom(ps => ps.NullableCustomTargetMdp))
          .ForMember(x => x.OverridingTargetPassCountRange,
            opt => opt.ResolveUsing<CustomTargetPassCountRangeResolver>())
          .ForMember(x => x.OverridingTemperatureWarningLevels,
            opt => opt.ResolveUsing<CustomTemperatureWarningLevelsResolver>())
          .ForMember(x => x.IncludeSupersededLifts,
            opt => opt.Ignore())//Raptor only uses this when using lifts (all layers). For 'no lift' is always true.
          .ForMember(x => x.LiftThicknessTarget,
            opt => opt.Ignore())
          .ForMember(x => x.MachineSpeedTarget,
            opt => opt.ResolveUsing<CustomMachineSpeedTargetResolver>());
      }
    }
  }
}
