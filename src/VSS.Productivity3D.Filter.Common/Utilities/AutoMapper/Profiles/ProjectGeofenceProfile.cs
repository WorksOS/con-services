using AutoMapper;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles
{
  public class ProjectGeofenceProfile : Profile
  {
    public ProjectGeofenceProfile()
    {
      CreateMap<ProjectGeofenceRequest, AssociateProjectGeofence>()
        .ForMember(x => x.GeofenceUID, opt => opt.MapFrom(src => src.BoundaryUid))
        .ForMember(x => x.ProjectUID, opt => opt.MapFrom(src => src.ProjectUid))
        .ForMember(x => x.ActionUTC, opt => opt.Ignore())
        .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
    }
  }
}