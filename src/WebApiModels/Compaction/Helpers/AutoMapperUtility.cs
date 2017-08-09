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
          //Note: CMV raw values are 10ths
          cfg.CreateMap<CompactionProjectSettings, CMVSettings>()
            .ForMember(x => x.overrideTargetCMV,
              opt => opt.MapFrom(ps => ps.useMachineTargetCmv.HasValue && !ps.useMachineTargetCmv.Value))
            .ForMember(x => x.cmvTarget,
              opt => opt.MapFrom(ps => (ps.customTargetCmv.HasValue ? ps.customTargetCmv.Value : CompactionProjectSettings.DefaultSettings.customTargetCmv) * 10))
            .ForMember(x => x.minCMV,
            opt => opt.UseValue(MIN_CMV_MDP_VALUE))
            .ForMember(x => x.maxCMV,
            opt => opt.UseValue(MAX_CMV_MDP_VALUE))
            .ForMember(x => x.minCMVPercent,
            opt => opt.MapFrom(ps => ps.customTargetCmvPercentMinimum.HasValue ? ps.customTargetCmvPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum.Value))
            .ForMember(x => x.maxCMVPercent,
            opt => opt.MapFrom(ps => ps.customTargetCmvPercentMaximum.HasValue ? ps.customTargetCmvPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum.Value));

          //Note: MDP raw values are 10ths
          cfg.CreateMap<CompactionProjectSettings, MDPSettings>()
            .ForMember(x => x.overrideTargetMDP,
              opt => opt.MapFrom(ps => ps.useMachineTargetMdp.HasValue && !ps.useMachineTargetMdp.Value))
            .ForMember(x => x.mdpTarget,
              opt => opt.MapFrom(ps => (ps.customTargetMdp.HasValue ? ps.customTargetMdp.Value : CompactionProjectSettings.DefaultSettings.customTargetMdp) * 10))
            .ForMember(x => x.minMDP,
              opt => opt.UseValue(MIN_CMV_MDP_VALUE))
            .ForMember(x => x.maxMDP,
              opt => opt.UseValue(MAX_CMV_MDP_VALUE))
            .ForMember(x => x.minMDPPercent,
              opt => opt.MapFrom(ps => ps.customTargetMdpPercentMinimum.HasValue ? ps.customTargetMdpPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum.Value))
            .ForMember(x => x.maxMDPPercent,
              opt => opt.MapFrom(ps => ps.customTargetMdpPercentMaximum.HasValue ? ps.customTargetMdpPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum.Value));

          //Note: TemperatureSettings are °C but TemperatureWarningLevels are 10ths of °C
          cfg.CreateMap<CompactionProjectSettings, TemperatureSettings>()
            .ForMember(x => x.overrideTemperatureRange,
              opt => opt.MapFrom(ps => ps.useMachineTargetTemperature.HasValue && !ps.useMachineTargetTemperature.Value))
            .ForMember(x => x.minTemperature,
              opt => opt.MapFrom(ps => ps.customTargetTemperatureMinimum.HasValue ? ps.customTargetTemperatureMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMinimum.Value))
            .ForMember(x => x.maxTemperature,
              opt => opt.MapFrom(ps => ps.customTargetTemperatureMaximum.HasValue ? ps.customTargetTemperatureMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMaximum.Value));

          cfg.CreateMap<CompactionProjectSettings, PassCountSettings>()
            .ForMember(x => x.passCounts,
              opt => opt.MapFrom(ps => ps.useDefaultPassCountTargets.HasValue && !ps.useDefaultPassCountTargets.Value && 
                                 ps.customPassCountTargets != null && ps.customPassCountTargets.Count > 0
                ? ps.customPassCountTargets.ToArray() : CompactionProjectSettings.DefaultSettings.customPassCountTargets.ToArray()));

          cfg.CreateMap<CompactionProjectSettings, CmvPercentChangeSettings>()
            .ForMember(x => x.percents,
              opt => opt.UseValue(new double[] { 5, 20, 50 }));

          //These are for LiftBuildSettings
          /*
          cfg.CreateMap<CompactionProjectSettings, CCVRangePercentage>()
            .ForMember(x => x.min,
              opt => opt.MapFrom(ps => ps.customTargetCmvPercentMinimum.HasValue ? ps.customTargetCmvPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum.Value))
            .ForMember(x => x.max,
              opt => opt.MapFrom(ps => ps.customTargetCmvPercentMaximum.HasValue ? ps.customTargetCmvPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum.Value));

          cfg.CreateMap<CompactionProjectSettings, MDPRangePercentage>()
            .ForMember(x => x.min,
              opt => opt.MapFrom(ps => ps.customTargetMdpPercentMinimum.HasValue ? ps.customTargetMdpPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum.Value))
            .ForMember(x => x.max,
              opt => opt.MapFrom(ps => ps.customTargetMdpPercentMaximum.HasValue ? ps.customTargetMdpPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum.Value));

          cfg.CreateMap<CompactionProjectSettings, TargetPassCountRange>()
            .ForMember(x => x.min,
              opt => opt.MapFrom(ps => ps.customTargetPassCountMinimum.HasValue ? ps.customTargetPassCountMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetPassCountMinimum.Value))
            .ForMember(x => x.max,
            opt => opt.MapFrom(ps => ps.customTargetPassCountMaximum.HasValue ? ps.customTargetPassCountMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetPassCountMaximum.Value));

          cfg.CreateMap<CompactionProjectSettings, TemperatureWarningLevels>()
            .ForMember(x => x.min,
              opt => opt.MapFrom(ps => (ps.customTargetTemperatureMinimum.HasValue ? ps.customTargetTemperatureMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMinimum.Value) * 10))
            .ForMember(x => x.max,
              opt => opt.MapFrom(ps => (ps.customTargetTemperatureMaximum.HasValue ? ps.customTargetTemperatureMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMaximum.Value) * 10)); 

          cfg.CreateMap<CompactionProjectSettings, MachineSpeedTarget>()
            .ForMember(x => x.MinTargetMachineSpeed,
              opt => opt.MapFrom(ps => (ps.useDefaultTargetRangeSpeed.HasValue && !ps.useDefaultTargetRangeSpeed.Value && ps.customTargetSpeedMinimum.HasValue ?
                                        ps.customTargetSpeedMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetSpeedMinimum.Value) * ConversionConstants.KM_HR_TO_CM_SEC))
            .ForMember(x => x.MaxTargetMachineSpeed,
            opt => opt.MapFrom(ps => (ps.useDefaultTargetRangeSpeed.HasValue && !ps.useDefaultTargetRangeSpeed.Value && ps.customTargetSpeedMaximum.HasValue ? 
                                      ps.customTargetSpeedMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetSpeedMaximum.Value) * ConversionConstants.KM_HR_TO_CM_SEC));
          */
          cfg.CreateMap<CompactionProjectSettings, LiftBuildSettings>()
            .ForMember(x => x.cCVRange,
              opt => opt.ResolveUsing<CustomCCVRangePercentageResolver>())
            .ForMember(x => x.cCVSummarizeTopLayerOnly,
              opt => opt.Ignore())
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
              opt => opt.Ignore())
            .ForMember(x => x.overridingLiftThickness,
              opt => opt.Ignore())
            .ForMember(x => x.overridingMachineCCV,
              opt => opt.MapFrom(ps => ps.useMachineTargetCmv.HasValue && !ps.useMachineTargetCmv.Value &&
                                       ps.customTargetCmv.HasValue
                ? (short) (ps.customTargetCmv.Value * 10)
                : (short?) null))
            .ForMember(x => x.overridingMachineMDP,
              opt => opt.MapFrom(ps => ps.useMachineTargetMdp.HasValue && !ps.useMachineTargetMdp.Value &&
                                       ps.customTargetMdp.HasValue
                ? (short) (ps.customTargetMdp.Value * 10)
                : (short?) null))
            .ForMember(x => x.overridingTargetPassCountRange,
              opt => opt.ResolveUsing<CustomTargetPassCountRangeResolver>())
            .ForMember(x => x.overridingTemperatureWarningLevels,
              opt => opt.ResolveUsing<CustomTemperatureWarningLevelsResolver>())
            .ForMember(x => x.includeSupersededLifts,
              opt => opt.Ignore())
            .ForMember(x => x.liftThicknessTarget,
              opt => opt.Ignore())
            .ForMember(x => x.machineSpeedTarget,
              opt => opt.ResolveUsing<CustomMachineSpeedTargetResolver>());
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
     
    }

    public static short MIN_CMV_MDP_VALUE = 0;
    public static short MAX_CMV_MDP_VALUE = 2000;

    public class CustomCCVRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, CCVRangePercentage>
    {
      public CCVRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, CCVRangePercentage member, ResolutionContext context)
      {
        var cmvOverrideRange = src.useDefaultTargetRangeCmvPercent.HasValue && !src.useDefaultTargetRangeCmvPercent.Value;
        var cmvMinPercent = src.customTargetCmvPercentMinimum.HasValue ? src.customTargetCmvPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum.Value;
        var cmvMaxPercent = src.customTargetCmvPercentMaximum.HasValue ? src.customTargetCmvPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum.Value;

        return cmvOverrideRange ? CCVRangePercentage.CreateCcvRangePercentage(cmvMinPercent, cmvMaxPercent) : null;
      }
    }

    public class CustomMDPRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MDPRangePercentage>
    {
      public MDPRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MDPRangePercentage member, ResolutionContext context)
      {
        var mdpOverrideRange = src.useDefaultTargetRangeMdpPercent.HasValue && !src.useDefaultTargetRangeMdpPercent.Value;
        var mdpMinPercent = src.customTargetMdpPercentMinimum.HasValue ? src.customTargetMdpPercentMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum.Value;
        var mdpMaxPercent = src.customTargetMdpPercentMaximum.HasValue ? src.customTargetMdpPercentMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum.Value;

        return mdpOverrideRange ? MDPRangePercentage.CreateMdpRangePercentage(mdpMinPercent, mdpMaxPercent) : null;
      }
    }

    public class CustomTargetPassCountRangeResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TargetPassCountRange>
    {
      public TargetPassCountRange Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TargetPassCountRange member, ResolutionContext context)
      {
        var passCountOverrideRange = src.useMachineTargetPassCount.HasValue && !src.useMachineTargetPassCount.Value;
        var passCountMin = src.customTargetPassCountMinimum.HasValue ? src.customTargetPassCountMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetPassCountMinimum.Value;
        var passCountMax = src.customTargetPassCountMaximum.HasValue ? src.customTargetPassCountMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetPassCountMaximum.Value;

        return passCountOverrideRange
          ? TargetPassCountRange.CreateTargetPassCountRange((ushort) passCountMin, (ushort) passCountMax)
          : null;
      }
    }
    public class CustomTemperatureWarningLevelsResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TemperatureWarningLevels>
    {
      public TemperatureWarningLevels Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TemperatureWarningLevels member, ResolutionContext context)
      {
        var overrideRange = src.useMachineTargetTemperature.HasValue && !src.useMachineTargetTemperature.Value;
        var tempMin = (src.customTargetTemperatureMinimum.HasValue ? src.customTargetTemperatureMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMinimum.Value) * 10;
        var tempMax = (src.customTargetTemperatureMaximum.HasValue ? src.customTargetTemperatureMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetTemperatureMaximum.Value) * 10;

        return overrideRange
          ? TemperatureWarningLevels.CreateTemperatureWarningLevels((ushort)Math.Round(tempMin), (ushort)Math.Round(tempMax)) : null;
      }
    }

    public class CustomMachineSpeedTargetResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MachineSpeedTarget>
    {
      public MachineSpeedTarget Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MachineSpeedTarget member, ResolutionContext context)
      {
        //Note: Speed is cm/s for Raptor but km/h in project settings
        var speedOverrideRange = src.useDefaultTargetRangeSpeed.HasValue && !src.useDefaultTargetRangeSpeed.Value;
        var speedMin = (speedOverrideRange && src.customTargetSpeedMinimum.HasValue ? src.customTargetSpeedMinimum.Value : CompactionProjectSettings.DefaultSettings.customTargetSpeedMinimum.Value) * ConversionConstants.KM_HR_TO_CM_SEC;
        var speedMax = (speedOverrideRange && src.customTargetSpeedMaximum.HasValue ? src.customTargetSpeedMaximum.Value : CompactionProjectSettings.DefaultSettings.customTargetSpeedMaximum.Value) * ConversionConstants.KM_HR_TO_CM_SEC;

        return MachineSpeedTarget.CreateMachineSpeedTarget((ushort)Math.Round(speedMin), (ushort)Math.Round(speedMax));
      }
    }

  }
}
