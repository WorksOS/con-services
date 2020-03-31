using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class FenceProfile : Profile
  {
    public FenceProfile()
    {
      CreateMap<Point, FencePoint>()
        .ForMember(x => x.X,
          opt => opt.MapFrom(f => f.x))
        .ForMember(x => x.Y,
          opt => opt.MapFrom(f => f.y))
        .ForMember(x => x.Z,
          opt => opt.MapFrom(src => 0));

      CreateMap<WGSPoint, FencePoint>()
        .ForMember(x => x.X,
          opt => opt.MapFrom(f => f.Lon))
        .ForMember(x => x.Y,
          opt => opt.MapFrom(f => f.Lat))
        .ForMember(x => x.Z,
          opt => opt.MapFrom(src => 0));
    }
  }
}
