using System;
using AutoMapper;
using ProjectWebApi.Models;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApiCommon.Utilities
{
  public class AutoMapperUtility
  {
    private static MapperConfiguration _automapperConfiguration;

    public static MapperConfiguration AutomapperConfiguration
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapperConfiguration;
      }
    }

    private static IMapper _automapper;

    public static IMapper Automapper
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapper;
      }
    }


    public static void ConfigureAutomapper()
    {
      _automapperConfiguration = new MapperConfiguration(
        //define mappings <source type, destination type>
        cfg =>
        {
          cfg.AllowNullCollections = true; // byte[] can be null
          cfg.CreateMap<CreateProjectRequest, CreateProjectEvent>()
            .ForMember(x => x.CustomerID, opt => opt.MapFrom(src => src.CustomerId ?? 0))
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
          cfg.CreateMap<UpdateProjectRequest, UpdateProjectEvent>()
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
          cfg.CreateMap<Project, ProjectV4Descriptor>()
            .ForMember(x => x.Description, opt => opt.Ignore()) // todo
            .ForMember(x => x.ProjectGeofenceWKT, opt => opt.MapFrom(src => src.GeometryWKT))
            .ForMember(x => x.ServiceType, opt => opt.MapFrom(src => src.ServiceTypeID))
            .ForMember(x => x.IsArchived, opt => opt.MapFrom(src => (src.IsDeleted || src.SubscriptionEndDate < DateTime.UtcNow)))
            .ForMember(x => x.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(x => x.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")))
            .ForMember(x => x.SubscriptionStartDate, opt => opt.MapFrom(src => (src.SubscriptionStartDate.HasValue ? src.SubscriptionStartDate.Value.ToString("O") : string.Empty)))
            .ForMember(x => x.SubscriptionEndDate, opt => opt.MapFrom(src => src.SubscriptionEndDate.HasValue ? src.SubscriptionEndDate.Value.ToString("O") : string.Empty))
          ;
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
