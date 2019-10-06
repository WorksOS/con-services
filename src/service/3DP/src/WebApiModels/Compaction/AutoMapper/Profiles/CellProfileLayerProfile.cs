using AutoMapper;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
#if RAPTOR
using SVOICDecls;
#endif


namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class CellProfileLayerProfile : Profile
  {
    public CellProfileLayerProfile()
    {
      CreateMap<CellPassesV2Result.ProfileLayer, CellPassesResult.ProfileLayer>()
        .ForMember(x => x.Amplitude,
          opt => opt.MapFrom(pl => pl.Amplitude))
        .ForMember(x => x.CCV,
          opt => opt.MapFrom(pl => pl.Ccv))
        .ForMember(x => x.CCV_Elev,
          opt => opt.MapFrom(pl => pl.CcvElev))
        .ForMember(x => x.CCV_MachineID,
          opt => opt.MapFrom(pl => pl.CcvMachineId))
        .ForMember(x => x.CCV_Time,
          opt => opt.MapFrom(pl => pl.CcvTime))
        .ForMember(x => x.FilteredHalfPassCount,
          opt => opt.MapFrom(pl => pl.FilteredHalfPassCount))
        .ForMember(x => x.FilteredPassCount,
          opt => opt.MapFrom(pl => pl.FilteredPassCount))
        .ForMember(x => x.FirstPassHeight,
          opt => opt.MapFrom(pl => pl.FirstPassHeight))
        .ForMember(x => x.Frequency,
          opt => opt.MapFrom(pl => pl.Frequency))
        .ForMember(x => x.Height,
          opt => opt.MapFrom(pl => pl.Height))
        .ForMember(x => x.LastLayerPassTime,
          opt => opt.MapFrom(pl => pl.LastLayerPassTime))
        .ForMember(x => x.LastPassHeight,
          opt => opt.MapFrom(pl => pl.LastPassHeight))
        .ForMember(x => x.MachineID,
          opt => opt.MapFrom(pl => pl.MachineId))
        .ForMember(x => x.MaterialTemperature,
          opt => opt.MapFrom(pl => pl.MaterialTemperature))
        .ForMember(x => x.MaterialTemperature_Elev,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureElev))
        .ForMember(x => x.MaterialTemperature_MachineID,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureMachineId))
        .ForMember(x => x.MaterialTemperature_Time,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureTime))
        .ForMember(x => x.MaximumPassHeight,
          opt => opt.MapFrom(pl => pl.MaximumPassHeight))
        .ForMember(x => x.MaxThickness,
          opt => opt.MapFrom(pl => pl.MaxThickness))
        .ForMember(x => x.MDP,
          opt => opt.MapFrom(pl => pl.Mdp))
        .ForMember(x => x.MDP_Elev,
          opt => opt.MapFrom(pl => pl.MdpElev))
        .ForMember(x => x.MDP_MachineID,
          opt => opt.MapFrom(pl => pl.MdpMachineId))
        .ForMember(x => x.MDP_Time,
          opt => opt.MapFrom(pl => pl.MdpTime))
        .ForMember(x => x.MinimumPassHeight,
          opt => opt.MapFrom(pl => pl.MinimumPassHeight))
        .ForMember(x => x.RadioLatency,
          opt => opt.MapFrom(pl => pl.RadioLatency))
        .ForMember(x => x.RMV,
          opt => opt.MapFrom(pl => pl.Rmv))
        .ForMember(x => x.TargetCCV,
          opt => opt.MapFrom(pl => pl.TargetCcv))
        .ForMember(x => x.TargetMDP,
          opt => opt.MapFrom(pl => pl.TargetMdp))
        .ForMember(x => x.TargetPassCount,
          opt => opt.MapFrom(pl => pl.TargetPassCount))
        .ForMember(x => x.TargetThickness,
          opt => opt.MapFrom(pl => pl.TargetThickness))
        .ForMember(x => x.Thickness,
          opt => opt.MapFrom(pl => pl.Thickness))
        .ForMember(x => x.FilteredPassData,
          opt => opt.MapFrom(pl => pl.PassData));
      CreateMap<CellPassesV2Result.FilteredPassData, CellPassesResult.FilteredPassData>()
        .ForMember(x => x.FilteredPass,
          opt => opt.MapFrom(fpd => fpd.FilteredPass))
        .ForMember(x => x.EventsValue,
          opt => opt.MapFrom(fpd => fpd.EventsValue))
        .ForMember(x => x.TargetsValue,
          opt => opt.MapFrom(fpd => fpd.TargetsValue));
      CreateMap<CellPassesV2Result.CellPassValue, CellPassesResult.CellPassValue>()
        .ForMember(x => x.Amplitude,
          opt => opt.MapFrom(cev => cev.Amplitude))
        .ForMember(x => x.CCV,
          opt => opt.MapFrom(cev => cev.Ccv))
        .ForMember(x => x.Frequency,
          opt => opt.MapFrom(cev => cev.Frequency))
        .ForMember(x => x.Height,
          opt => opt.MapFrom(cev => cev.Height))
        .ForMember(x => x.MachineID,
          opt => opt.MapFrom(cev => cev.MachineId))
        .ForMember(x => x.MachineSpeed,
          opt => opt.MapFrom(cev => cev.MachineSpeed))
        .ForMember(x => x.MaterialTemperature,
          opt => opt.MapFrom(cev => cev.MaterialTemperature))
        .ForMember(x => x.MDP,
          opt => opt.MapFrom(cev => cev.Mdp))
        .ForMember(x => x.RadioLatency,
          opt => opt.MapFrom(cev => cev.RadioLatency))
        .ForMember(x => x.RMV,
          opt => opt.MapFrom(cev => cev.Rmv))
        .ForMember(x => x.Time,
          opt => opt.MapFrom(cev => cev.Time))
        .ForMember(x => x.GPSModeStore,
          opt => opt.MapFrom(cev => cev.GpsModeStore));
      CreateMap<CellPassesV2Result.CellEventsValue, CellPassesResult.CellEventsValue>()
        .ForMember(x => x.EventAutoVibrationState,
          opt => opt.MapFrom(cev => cev.EventAutoVibrationState))
        .ForMember(x => x.EventDesignNameID,
          opt => opt.MapFrom(cev => cev.EventDesignNameId))
        .ForMember(x => x.EventICFlags,
          opt => opt.MapFrom(cev => cev.EventIcFlags))
        .ForMember(x => x.EventMachineAutomatics,
          opt => opt.MapFrom(cev => cev.EventMachineAutomatics))
        .ForMember(x => x.EventMachineGear,
          opt => opt.MapFrom(cev => cev.EventMachineGear))
        .ForMember(x => x.EventMachineRMVThreshold,
          opt => opt.MapFrom(cev => cev.EventMachineRmvThreshold))
        .ForMember(x => x.EventOnGroundState,
          opt => opt.MapFrom(cev => cev.EventOnGroundState))
        .ForMember(x => x.EventVibrationState,
          opt => opt.MapFrom(cev => cev.EventVibrationState))
        .ForMember(x => x.GPSAccuracy,
          opt => opt.MapFrom(cev => cev.GpsAccuracy))
        .ForMember(x => x.GPSTolerance,
          opt => opt.MapFrom(cev => cev.GpsTolerance))
        .ForMember(x => x.LayerID,
          opt => opt.MapFrom(cev => cev.LayerId))
        .ForMember(x => x.MapReset_DesignNameID,
          opt => opt.MapFrom(cev => cev.MapResetDesignNameId))
        .ForMember(x => x.MapReset_PriorDate,
          opt => opt.MapFrom(cev => cev.MapResetPriorDate))
        .ForMember(x => x.PositioningTech,
          opt => opt.MapFrom(cev => cev.PositioningTech))
        .ForMember(x => x.EventInAvoidZoneState,
          opt => opt.MapFrom(cev => cev.EventInAvoidZoneState))
#if RAPTOR
        .ForMember(x => x.EventMinElevMapping,
          opt => opt.MapFrom(cev => (TICMinElevMappingState)cev.EventMinElevMapping));
#else
        .ForMember(x => x.EventMinElevMapping,
          opt => opt.MapFrom(cev => cev.EventMinElevMapping));
#endif
      CreateMap<CellPassesV2Result.CellTargetsValue, CellPassesResult.CellTargetsValue>()
        .ForMember(x => x.TargetCCV,
          opt => opt.MapFrom(ctv => ctv.TargetCcv))
        .ForMember(x => x.TargetMDP,
          opt => opt.MapFrom(ctv => ctv.TargetMdp))
        .ForMember(x => x.TargetPassCount,
          opt => opt.MapFrom(ctv => ctv.TargetPassCount))
        .ForMember(x => x.TargetThickness,
          opt => opt.MapFrom(ctv => ctv.TargetThickness))
        .ForMember(x => x.TempWarningLevelMin,
          opt => opt.MapFrom(ctv => ctv.TempWarningLevelMin))
        .ForMember(x => x.TempWarningLevelMax,
          opt => opt.MapFrom(ctv => ctv.TempWarningLevelMax));
    }
  }
}
