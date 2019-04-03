using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class PointProfile : Profile
  {
    public PointProfile()
    {
      CreateMap<WGSPoint, XYZ>()
        .ForMember(x => x.Y,
          opt => opt.MapFrom(f => f.Lat))
        .ForMember(x => x.X,
          opt => opt.MapFrom(f => f.Lon))
        .ForMember(x => x.Z,
          opt => opt.UseValue(0));

      CreateMap<Point, XYZ>()
        .ForMember(x => x.X,
          opt => opt.MapFrom(f => f.x))
        .ForMember(x => x.Y,
          opt => opt.MapFrom(f => f.y))
        .ForMember(x => x.Z,
          opt => opt.UseValue(0));
    }
  }
}
