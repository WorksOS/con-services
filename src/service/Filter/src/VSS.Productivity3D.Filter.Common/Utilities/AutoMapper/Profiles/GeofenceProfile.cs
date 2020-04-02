using System;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles
{
  public class GeofenceProfile : Profile
  {
    public GeofenceProfile()
    {
      CreateMap<GeofenceData, Geofence>()
        .ForMember(x => x.GeofenceUID, opt => opt.MapFrom(src => src.GeofenceUID.ToString()))
        .ForMember(x => x.GeofenceType,
          opt => opt.MapFrom(src => (GeofenceType) Enum.Parse(typeof(GeofenceType), src.GeofenceType, true)))
        .ForMember(x => x.GeometryWKT, opt => opt.MapFrom(src => src.GeometryWKT))
        .ForMember(x => x.Name, opt => opt.MapFrom(src => src.GeofenceName))
        .ForMember(x => x.FillColor, opt => opt.MapFrom(src => src.FillColor))
        .ForMember(x => x.IsTransparent, opt => opt.MapFrom(src => src.IsTransparent))
        .ForMember(x => x.CustomerUID, opt => opt.MapFrom(src => src.CustomerUID))
        .ForMember(x => x.UserUID, opt => opt.MapFrom(src => src.UserUID))
        .ForMember(x => x.AreaSqMeters, opt => opt.MapFrom(src => src.AreaSqMeters))
        .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
        .ForMember(x => x.LastActionedUTC, opt => opt.MapFrom(src => src.ActionUTC))
        .ForMember(x => x.IsDeleted, opt => opt.MapFrom(src => false));
    }
  }
}
