using AutoMapper;
using VSS.MasterData.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class BoundingWorldExtent3DProfile : Profile
  {
    public BoundingWorldExtent3DProfile()
    {
      CreateMap<BoundingBox2DGrid, BoundingWorldExtent3D>()
        .ForMember(x => x.MinX,
          opt => opt.MapFrom(f => f.BottomLeftX))
        .ForMember(x => x.MinY,
          opt => opt.MapFrom(f => f.BottomleftY))
        .ForMember(x => x.MinZ,
          opt => opt.UseValue(0))
        .ForMember(x => x.MaxX,
          opt => opt.MapFrom(f => f.TopRightX))
        .ForMember(x => x.MaxY,
          opt => opt.MapFrom(f => f.TopRightY))
        .ForMember(x => x.MaxZ,
          opt => opt.UseValue(0));

      CreateMap<BoundingBox2DLatLon, BoundingWorldExtent3D>()
        .ForMember(x => x.MinX,
          opt => opt.MapFrom(f => f.BottomLeftLon))
        .ForMember(x => x.MinY,
          opt => opt.MapFrom(f => f.BottomLeftLat))
        .ForMember(x => x.MinZ,
          opt => opt.UseValue(0))
        .ForMember(x => x.MaxX,
          opt => opt.MapFrom(f => f.TopRightLon))
        .ForMember(x => x.MaxY,
          opt => opt.MapFrom(f => f.TopRightLat))
        .ForMember(x => x.MaxZ,
          opt => opt.UseValue(0));
    }
  }
}
