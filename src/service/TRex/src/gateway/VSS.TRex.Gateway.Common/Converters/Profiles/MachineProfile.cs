using System;
using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Machines.Interfaces;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class MachineProfile : Profile
  {
    public MachineProfile()
    {
      CreateMap<IMachine, MachineStatus>()
        .ForMember(x => x.AssetId,
          opt => opt.UseValue(-1))
        .ForMember(x => x.AssetUid,
          opt => opt.MapFrom(f => f.ID))
        .ForMember(x => x.MachineName,
          opt => opt.MapFrom(f => f.Name))
        .ForMember(x => x.IsJohnDoe,
          opt => opt.MapFrom(f => f.IsJohnDoeMachine))
        .ForMember(x => x.lastKnownTimeStamp,
          opt => opt.MapFrom(f => f.LastKnownPositionTimeStamp))
        .ForMember(x => x.lastKnownLongitude,
        // todoJeannie convert from x/y opt => opt.MapFrom(f => f.LastKnownX))
        opt => opt.UseValue(Double.MaxValue))
        .ForMember(x => x.lastKnownLatitude,
        // todoJeannie convert from x/y opt => opt.MapFrom(f => f.LastKnownY))
        opt => opt.UseValue(Double.MaxValue))
        ;
    }
  }
}
