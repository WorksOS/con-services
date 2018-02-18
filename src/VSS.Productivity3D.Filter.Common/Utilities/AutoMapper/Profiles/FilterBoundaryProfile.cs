using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles
{
  public class FilterBoundaryProfile : Profile
  {
    public FilterBoundaryProfile()
    {
      CreateMap<Geofence, GeofenceData>()
        .ForMember(dest => dest.GeofenceUID, opt => opt.MapFrom(src => src.GeofenceUID))
        .ForMember(dest => dest.GeofenceName, opt => opt.MapFrom(src => src.Name))
        .ForMember(dest => dest.GeofenceType, opt => opt.MapFrom(src => src.GeofenceType))
        .ForMember(dest => dest.GeometryWKT, opt => opt.MapFrom(src => src.GeometryWKT))
        .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.CustomerUID))
        .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.UserUID))
        .ForMember(dest => dest.Description, opt => opt.Ignore())
        .ForMember(dest => dest.FillColor, opt => opt.Ignore())
        .ForMember(dest => dest.IsTransparent, opt => opt.Ignore())
        .ForMember(dest => dest.ActionUTC, opt => opt.Ignore());

      CreateMap<BoundaryRequestFull, CreateGeofenceEvent>()
        .ForMember(dest => dest.GeofenceName, opt => opt.MapFrom(src => src.Request.Name))
        .ForMember(dest => dest.GeofenceUID, opt => opt.MapFrom(src => src.Request.BoundaryUid))
        .ForMember(dest => dest.GeometryWKT, opt => opt.MapFrom(src => src.Request.BoundaryPolygonWKT))
        .ForMember(dest => dest.GeofenceType, opt => opt.MapFrom(src => src.GeofenceType.ToString()))
        .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.UserUid))
        .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.CustomerUid))
        .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
        .ForMember(dest => dest.ReceivedUTC, opt => opt.Ignore())
        .ForMember(dest => dest.Description, opt => opt.Ignore())
        .ForMember(dest => dest.FillColor, opt => opt.UseValue(16011550))//F4511E
        .ForMember(dest => dest.IsTransparent, opt => opt.UseValue(true))
        .ForMember(dest => dest.EndDate, opt => opt.Ignore())
        .ForMember(dest => dest.AreaSqMeters, opt => opt.Ignore());

      CreateMap<BoundaryRequestFull, UpdateGeofenceEvent>()
        .ForMember(dest => dest.GeofenceName, opt => opt.MapFrom(src => src.Request.Name))
        .ForMember(dest => dest.GeofenceUID, opt => opt.MapFrom(src => src.Request.BoundaryUid))
        .ForMember(dest => dest.GeometryWKT, opt => opt.MapFrom(src => src.Request.BoundaryPolygonWKT))
        .ForMember(dest => dest.GeofenceType, opt => opt.MapFrom(src => src.GeofenceType.ToString()))
        .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.UserUid))
        .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
        .ForMember(dest => dest.ReceivedUTC, opt => opt.Ignore())
        .ForMember(dest => dest.Description, opt => opt.Ignore())
        .ForMember(dest => dest.FillColor, opt => opt.Ignore())
        .ForMember(dest => dest.IsTransparent, opt => opt.Ignore())
        .ForMember(dest => dest.EndDate, opt => opt.Ignore())
        .ForMember(dest => dest.AreaSqMeters, opt => opt.Ignore());

      CreateMap<BoundaryUidRequestFull, DeleteGeofenceEvent>()
        .ForMember(dest => dest.GeofenceUID, opt => opt.MapFrom(src => src.BoundaryUid))
        .ForMember(dest => dest.UserUID, opt => opt.MapFrom(src => src.UserUid))
        .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
        .ForMember(dest => dest.ReceivedUTC, opt => opt.Ignore());
    }
  }
}