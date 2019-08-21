using AutoMapper;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
#if RAPTOR
using SVOICDecls;
#endif


namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class ProfileLayerProfile : Profile
  {
    public ProfileLayerProfile()
    {
      CreateMap<CellPassesV2Result.ProfileLayer, CellPassesResult.ProfileLayer>()
        .ForMember(x => x.amplitude,
          opt => opt.MapFrom(pl => pl.Amplitude))
        .ForMember(x => x.cCV,
          opt => opt.MapFrom(pl => pl.Ccv))
        .ForMember(x => x.cCV_Elev,
          opt => opt.MapFrom(pl => pl.CcvElev))
        .ForMember(x => x.cCV_MachineID,
          opt => opt.MapFrom(pl => pl.CcvMachineId))
        .ForMember(x => x.cCV_Time,
          opt => opt.MapFrom(pl => pl.CcvTime))
        .ForMember(x => x.filteredHalfPassCount,
          opt => opt.MapFrom(pl => pl.FilteredHalfPassCount))
        .ForMember(x => x.filteredPassCount,
          opt => opt.MapFrom(pl => pl.FilteredPassCount))
        .ForMember(x => x.firstPassHeight,
          opt => opt.MapFrom(pl => pl.FirstPassHeight))
        .ForMember(x => x.frequency,
          opt => opt.MapFrom(pl => pl.Frequency))
        .ForMember(x => x.height,
          opt => opt.MapFrom(pl => pl.Height))
        .ForMember(x => x.lastLayerPassTime,
          opt => opt.MapFrom(pl => pl.LastLayerPassTime))
        .ForMember(x => x.lastPassHeight,
          opt => opt.MapFrom(pl => pl.LastPassHeight))
        .ForMember(x => x.machineID,
          opt => opt.MapFrom(pl => pl.MachineId))
        .ForMember(x => x.materialTemperature,
          opt => opt.MapFrom(pl => pl.MaterialTemperature))
        .ForMember(x => x.materialTemperature_Elev,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureElev))
        .ForMember(x => x.materialTemperature_MachineID,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureMachineId))
        .ForMember(x => x.materialTemperature_Time,
          opt => opt.MapFrom(pl => pl.MaterialTemperatureTime))
        .ForMember(x => x.maximumPassHeight,
          opt => opt.MapFrom(pl => pl.MaximumPassHeight))
        .ForMember(x => x.maxThickness,
          opt => opt.MapFrom(pl => pl.MaxThickness))
        .ForMember(x => x.mDP,
          opt => opt.MapFrom(pl => pl.Mdp))
        .ForMember(x => x.mDP_Elev,
          opt => opt.MapFrom(pl => pl.MdpElev))
        .ForMember(x => x.mDP_MachineID,
          opt => opt.MapFrom(pl => pl.MdpMachineId))
        .ForMember(x => x.mDP_Time,
          opt => opt.MapFrom(pl => pl.MdpTime))
        .ForMember(x => x.minimumPassHeight,
          opt => opt.MapFrom(pl => pl.MinimumPassHeight))
        .ForMember(x => x.radioLatency,
          opt => opt.MapFrom(pl => pl.RadioLatency))
        .ForMember(x => x.rMV,
          opt => opt.MapFrom(pl => pl.Rmv))
        .ForMember(x => x.targetCCV,
          opt => opt.MapFrom(pl => pl.TargetCcv))
        .ForMember(x => x.targetMDP,
          opt => opt.MapFrom(pl => pl.TargetMdp))
        .ForMember(x => x.targetPassCount,
          opt => opt.MapFrom(pl => pl.TargetPassCount))
        .ForMember(x => x.targetThickness,
          opt => opt.MapFrom(pl => pl.TargetThickness))
        .ForMember(x => x.thickness,
          opt => opt.MapFrom(pl => pl.Thickness))
        .ForMember(x => x.filteredPassData,
          opt => opt.MapFrom(pl => pl.PassData));
      CreateMap<CellPassesV2Result.FilteredPassData, CellPassesResult.FilteredPassData>()
        .ForMember(x => x.filteredPass,
          opt => opt.MapFrom(fpd => fpd.FilteredPass))
        .ForMember(x => x.eventsValue,
          opt => opt.MapFrom(fpd => fpd.EventsValue))
        .ForMember(x => x.targetsValue,
          opt => opt.MapFrom(fpd => fpd.TargetsValue));
      CreateMap<CellPassesV2Result.CellPassValue, CellPassesResult.CellPassValue>()
        .ForMember(x => x.amplitude,
          opt => opt.MapFrom(cev => cev.Amplitude))
        .ForMember(x => x.cCV,
          opt => opt.MapFrom(cev => cev.Ccv))
        .ForMember(x => x.frequency,
          opt => opt.MapFrom(cev => cev.Frequency))
        .ForMember(x => x.height,
          opt => opt.MapFrom(cev => cev.Height))
        .ForMember(x => x.machineID,
          opt => opt.MapFrom(cev => cev.MachineId))
        .ForMember(x => x.machineSpeed,
          opt => opt.MapFrom(cev => cev.MachineSpeed))
        .ForMember(x => x.materialTemperature,
          opt => opt.MapFrom(cev => cev.MaterialTemperature))
        .ForMember(x => x.mDP,
          opt => opt.MapFrom(cev => cev.Mdp))
        .ForMember(x => x.radioLatency,
          opt => opt.MapFrom(cev => cev.RadioLatency))
        .ForMember(x => x.rMV,
          opt => opt.MapFrom(cev => cev.Rmv))
        .ForMember(x => x.time,
          opt => opt.MapFrom(cev => cev.Time))
        .ForMember(x => x.gPSModeStore,
          opt => opt.MapFrom(cev => cev.GpsModeStore));
      CreateMap<CellPassesV2Result.CellEventsValue, CellPassesResult.CellEventsValue>()
        .ForMember(x => x.eventAutoVibrationState,
          opt => opt.MapFrom(cev => cev.EventAutoVibrationState))
        .ForMember(x => x.eventDesignNameID,
          opt => opt.MapFrom(cev => cev.EventDesignNameId))
        .ForMember(x => x.eventICFlags,
          opt => opt.MapFrom(cev => cev.EventIcFlags))
        .ForMember(x => x.eventMachineAutomatics,
          opt => opt.MapFrom(cev => cev.EventMachineAutomatics))
        .ForMember(x => x.eventMachineGear,
          opt => opt.MapFrom(cev => cev.EventMachineGear))
        .ForMember(x => x.eventMachineRMVThreshold,
          opt => opt.MapFrom(cev => cev.EventMachineRmvThreshold))
        .ForMember(x => x.eventOnGroundState,
          opt => opt.MapFrom(cev => cev.EventOnGroundState))
        .ForMember(x => x.eventVibrationState,
          opt => opt.MapFrom(cev => cev.EventVibrationState))
        .ForMember(x => x.gPSAccuracy,
          opt => opt.MapFrom(cev => cev.GpsAccuracy))
        .ForMember(x => x.gPSTolerance,
          opt => opt.MapFrom(cev => cev.GpsTolerance))
        .ForMember(x => x.layerID,
          opt => opt.MapFrom(cev => cev.LayerId))
        .ForMember(x => x.mapReset_DesignNameID,
          opt => opt.MapFrom(cev => cev.MapResetDesignNameId))
        .ForMember(x => x.mapReset_PriorDate,
          opt => opt.MapFrom(cev => cev.MapResetPriorDate))
        .ForMember(x => x.positioningTech,
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
        .ForMember(x => x.targetCCV,
          opt => opt.MapFrom(ctv => ctv.TargetCcv))
        .ForMember(x => x.targetMDP,
          opt => opt.MapFrom(ctv => ctv.TargetMdp))
        .ForMember(x => x.targetPassCount,
          opt => opt.MapFrom(ctv => ctv.TargetPassCount))
        .ForMember(x => x.targetThickness,
          opt => opt.MapFrom(ctv => ctv.TargetThickness))
        .ForMember(x => x.tempWarningLevelMin,
          opt => opt.MapFrom(ctv => ctv.TempWarningLevelMin))
        .ForMember(x => x.tempWarningLevelMax,
          opt => opt.MapFrom(ctv => ctv.TempWarningLevelMax));
    }
  }
}
