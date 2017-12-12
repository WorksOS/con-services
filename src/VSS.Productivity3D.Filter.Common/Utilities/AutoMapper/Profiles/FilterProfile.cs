using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles
{
  public class FilterProfile : Profile
  {
    public FilterProfile()
    {
      CreateMap<MasterData.Repositories.DBModels.Filter, FilterDescriptor>();
      CreateMap<FilterRequestFull, CreateFilterEvent>()
        .ForMember(x => x.ActionUTC, opt => opt.Ignore())
        .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
#pragma warning disable CS0612 // Type or member is obsolete
        .ForMember(x => x.UserUID, opt => opt.Ignore());

#pragma warning restore CS0612 // Type or member is obsolete
      CreateMap<CreateFilterEvent, FilterDescriptor>();
#pragma warning disable CS0612 // Type or member is obsolete

#pragma warning restore CS0612 // Type or member is obsolete
      CreateMap<UpdateFilterEvent, FilterDescriptor>();
#pragma warning disable CS0612 // Type or member is obsolete

#pragma warning restore CS0612 // Type or member is obsolete
      CreateMap<FilterRequest, CreateFilterEvent>()
        .ForMember(x => x.ActionUTC, opt => opt.Ignore())
        .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
#pragma warning disable CS0612 // Type or member is obsolete
        .ForMember(x => x.UserUID, opt => opt.Ignore())
#pragma warning restore CS0612 // Type or member is obsolete
        .ForMember(x => x.CustomerUID, opt => opt.Ignore())
        .ForMember(x => x.ProjectUID, opt => opt.Ignore())
        .ForMember(x => x.UserID, opt => opt.Ignore());

      CreateMap<FilterRequestFull, UpdateFilterEvent>()
        .ForMember(x => x.ActionUTC, opt => opt.Ignore())
        .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
#pragma warning disable CS0612 // Type or member is obsolete
        .ForMember(x => x.UserUID, opt => opt.Ignore());

#pragma warning restore CS0612 // Type or member is obsolete
      CreateMap<FilterRequestFull, DeleteFilterEvent>()
        .ForMember(x => x.ActionUTC, opt => opt.Ignore())
        .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
#pragma warning disable CS0612 // Type or member is obsolete
        .ForMember(x => x.UserUID, opt => opt.Ignore());

#pragma warning restore CS0612 // Type or member is obsolete
      CreateMap<MasterData.Repositories.DBModels.Filter, DeleteFilterEvent>()
        .ForMember(x => x.ActionUTC, opt => opt.MapFrom(src => src.LastActionedUtc))
        .ForMember(x => x.ReceivedUTC, opt => opt.MapFrom(src => src.LastActionedUtc))
#pragma warning disable CS0612 // Type or member is obsolete
        .ForMember(x => x.UserUID, opt => opt.Ignore());

#pragma warning restore CS0612 // Type or member is obsolete
    }
  }
}