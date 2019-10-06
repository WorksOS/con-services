using System;
using AutoMapper;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
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
        // Lat/long should be set outside of this simple mapper as it requires call to another service
        .ForMember(x => x.lastKnownLongitude,
         opt => opt.UseValue(Double.MaxValue))
        .ForMember(x => x.lastKnownLatitude,
        opt => opt.UseValue(Double.MaxValue))
        ;
    }
  }
}
