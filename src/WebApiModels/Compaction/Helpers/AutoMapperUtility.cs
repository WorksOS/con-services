using System;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
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
          CCVRangePercentage.CreateCcvRangePercentage(src.CustomTargetCmvPercentMinimum, src.CustomTargetCmvPercentMaximum) :
          CCVRangePercentage.CreateCcvRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMaximum);
      }
    }

    public class CustomMDPRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MDPRangePercentage>
    {
      public MDPRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MDPRangePercentage member, ResolutionContext context)
      {
        return src.OverrideDefaultTargetRangeMdpPercent ? 
          MDPRangePercentage.CreateMdpRangePercentage(src.CustomTargetMdpPercentMinimum, src.CustomTargetMdpPercentMaximum) :
          MDPRangePercentage.CreateMdpRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetMdpPercentMaximum);
      }
    }

    public class CustomTargetPassCountRangeResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TargetPassCountRange>
    {
      public TargetPassCountRange Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TargetPassCountRange member, ResolutionContext context)
      {
        return src.OverrideMachineTargetPassCount
          ? TargetPassCountRange.CreateTargetPassCountRange((ushort) src.customTargetPassCountMinimum, (ushort) src.customTargetPassCountMaximum)
          : null;
      }
    }
    public class CustomTemperatureWarningLevelsResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TemperatureWarningLevels>
    {
      public TemperatureWarningLevels Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TemperatureWarningLevels member, ResolutionContext context)
      {
        return src.OverrideMachineTargetTemperature
          ? TemperatureWarningLevels.CreateTemperatureWarningLevels(src.CustomTargetTemperatureWarningLevelMinimum, src.CustomTargetTemperatureWarningLevelMaximum) : null;
      }
    }

    public class CustomMachineSpeedTargetResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MachineSpeedTarget>
    {
      public MachineSpeedTarget Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MachineSpeedTarget member, ResolutionContext context)
      {
        return MachineSpeedTarget.CreateMachineSpeedTarget(src.CustomTargetSpeedMinimum, src.CustomTargetSpeedMaximum);
      }
    }

    public class CmvSettingsProfile : Profile
    {
      public CmvSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, CMVSettings>()
          .ForMember(x => x.overrideTargetCMV,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetCmv))
          .ForMember(x => x.cmvTarget,
            opt => opt.MapFrom(ps => ps.CustomTargetCmv))
          .ForMember(x => x.minCMV,
            opt => opt.MapFrom(ps => ps.CmvMinimum))
          .ForMember(x => x.maxCMV,
            opt => opt.MapFrom(ps => ps.CmvMaximum))
          .ForMember(x => x.minCMVPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMinimum))
          .ForMember(x => x.maxCMVPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetCmvPercentMaximum));
      }
    }

    public class CmvSettingsExProfile : Profile
    {
      public CmvSettingsExProfile()
      {
        CreateMap<CompactionProjectSettings, CMVSettingsEx>()
          .IncludeBase<CompactionProjectSettings, CMVSettings>()
          .ForMember(x => x.customCMVDetailTargets,
            opt => opt.MapFrom(ps => ps.CustomCMVs));
      }
    }

    public class MdpSettingsProfile : Profile
    {
      public MdpSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, MDPSettings>()
          .ForMember(x => x.overrideTargetMDP,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetMdp))
          .ForMember(x => x.mdpTarget,
            opt => opt.MapFrom(ps => ps.CustomTargetMdp))
          .ForMember(x => x.minMDP,
            opt => opt.MapFrom(ps => ps.MdpMinimum))
          .ForMember(x => x.maxMDP,
            opt => opt.MapFrom(ps => ps.MdpMaximum))
          .ForMember(x => x.minMDPPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMinimum))
          .ForMember(x => x.maxMDPPercent,
            opt => opt.MapFrom(ps => ps.CustomTargetMdpPercentMaximum));
      }
    }

    public class TemperatureSettingsProfile : Profile
    {
      public TemperatureSettingsProfile()
      {
        CreateMap<CompactionProjectSettings, TemperatureSettings>()
          .ForMember(x => x.overrideTemperatureRange,
            opt => opt.MapFrom(ps => ps.OverrideMachineTargetTemperature))
          .ForMember(x => x.minTemperature,
            opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMinimum))
          .ForMember(x => x.maxTemperature,
            opt => opt.MapFrom(ps => ps.CustomTargetTemperatureMaximum));
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
          .ForMember(x => x.cCVRange,
            opt => opt.ResolveUsing<CustomCCVRangePercentageResolver>())
          .ForMember(x => x.cCVSummarizeTopLayerOnly,
            opt => opt.Ignore())//Raptor only uses this when using lifts (all layers)
          .ForMember(x => x.CCvSummaryType,
            opt => opt.Ignore())
          .ForMember(x => x.deadBandLowerBoundary,
            opt => opt.Ignore())
          .ForMember(x => x.deadBandUpperBoundary,
            opt => opt.Ignore())
          .ForMember(x => x.firstPassThickness,
            opt => opt.Ignore())
          .ForMember(x => x.liftDetectionType,
            opt => opt.UseValue(LiftDetectionType.None))
          .ForMember(x => x.liftThicknessType,
            opt => opt.UseValue(LiftThicknessType.Compacted))
          .ForMember(x => x.mDPRange,
            opt => opt.ResolveUsing<CustomMDPRangePercentageResolver>())
          .ForMember(x => x.mDPSummarizeTopLayerOnly,
            opt => opt.Ignore())//Raptor only uses this when using lifts (all layers)
          .ForMember(x => x.overridingLiftThickness,
            opt => opt.Ignore())
          .ForMember(x => x.overridingMachineCCV,
            opt => opt.MapFrom(ps => ps.NullableCustomTargetCmv))
          .ForMember(x => x.overridingMachineMDP,
            opt => opt.MapFrom(ps => ps.NullableCustomTargetMdp))
          .ForMember(x => x.overridingTargetPassCountRange,
            opt => opt.ResolveUsing<CustomTargetPassCountRangeResolver>())
          .ForMember(x => x.overridingTemperatureWarningLevels,
            opt => opt.ResolveUsing<CustomTemperatureWarningLevelsResolver>())
          .ForMember(x => x.includeSupersededLifts,
            opt => opt.Ignore())//Raptor only uses this when using lifts (all layers). For 'no lift' is always true.
          .ForMember(x => x.liftThicknessTarget,
            opt => opt.Ignore())
          .ForMember(x => x.machineSpeedTarget,
            opt => opt.ResolveUsing<CustomMachineSpeedTargetResolver>());
      }
    }
  }
}
