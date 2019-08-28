using System;
using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class OverrideParametersProfile : Profile
  {
    public OverrideParametersProfile()
    {
      const ushort TEMPERATURE_CONVERSION_FACTOR = 10;

      CreateMap<OverridingTargets, IOverrideParameters>()
        //Tell AutoMapper how to make the instance as it's mock for interfaces doesn't handle methods only properties.
        //Also note for interfaces can't have custom resolvers.
        .ConstructUsing( o => new OverrideParameters()) //
        .ForMember(x => x.OverrideMachineCCV, opt => opt.MapFrom(o => o.OverrideTargetCMV))
        .ForMember(x => x.OverridingMachineCCV, opt => opt.MapFrom(o => o.CmvTarget))
        .ForMember(x => x.CMVRange, 
          opt => opt.ResolveUsing( o => new CMVRangePercentageRecord(o.MinCMVPercent, o.MaxCMVPercent)))
        .ForMember(x => x.OverrideMachineMDP, opt => opt.MapFrom(o => o.OverrideTargetMDP))
        .ForMember(x => x.OverridingMachineMDP, opt => opt.MapFrom(o => o.MdpTarget))
        .ForMember(x => x.MDPRange, 
          opt => opt.ResolveUsing(o => new MDPRangePercentageRecord(o.MinMDPPercent, o.MaxMDPPercent)))
        .ForMember(x => x.OverrideTargetPassCount, opt => opt.MapFrom(o => o.OverridingTargetPassCountRange != null))
        .ForMember(x => x.OverridingTargetPassCountRange, 
          opt => opt.ResolveUsing(o => new PassCountRangeRecord(
            o.OverridingTargetPassCountRange?.Min ?? (ushort)0, 
            o.OverridingTargetPassCountRange?.Max ?? (ushort)0)))
        .ForMember(x => x.OverrideTemperatureWarningLevels,
          opt => opt.MapFrom(o => o.TemperatureSettings != null && o.TemperatureSettings.OverrideTemperatureRange))
        .ForMember(x => x.OverridingTemperatureWarningLevels, 
          opt => opt.ResolveUsing(o => new TemperatureWarningLevelsRecord(
            (ushort)((o.TemperatureSettings?.OverrideTemperatureRange ?? false) ? o.TemperatureSettings.MinTemperature * TEMPERATURE_CONVERSION_FACTOR : 0), 
            (ushort)((o.TemperatureSettings?.OverrideTemperatureRange ?? false) ? o.TemperatureSettings.MaxTemperature * TEMPERATURE_CONVERSION_FACTOR : CellPassConsts.MaxMaterialTempValue))))
        .ForMember(x => x.TargetMachineSpeed, 
          opt => opt.ResolveUsing(o => new MachineSpeedExtendedRecord(
            o.MachineSpeedTarget?.MinTargetMachineSpeed ?? 0, 
            o.MachineSpeedTarget?.MaxTargetMachineSpeed ?? 0)));
    }

  }
}
