using System;
using AutoMapper;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
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
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.CreateMap<CreateProjectRequest, CreateProjectEvent>()
            .ForMember(x => x.CustomerID, opt => opt.MapFrom(src => src.CustomerID ?? 0))
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
            .ForMember(x => x.ProjectID, opt => opt.Ignore());
          cfg.CreateMap<UpdateProjectRequest, UpdateProjectEvent>()
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
            .ForMember(x => x.ProjectTimezone, opt => opt.Ignore());
          cfg.CreateMap<Repositories.DBModels.Project, ProjectV4Descriptor>()
            .ForMember(x => x.ProjectGeofenceWKT, opt => opt.MapFrom(src => src.GeometryWKT))
            .ForMember(x => x.ServiceType, opt => opt.MapFrom(src => src.ServiceTypeID))
            .ForMember(x => x.IanaTimeZone, opt => opt.MapFrom(src => src.LandfillTimeZone))
            .ForMember(x => x.IsArchived,
              opt => opt.MapFrom(src => src.IsDeleted || src.SubscriptionEndDate < DateTime.UtcNow))
            .ForMember(x => x.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(x => x.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")))
            .ForMember(x => x.SubscriptionStartDate,
              opt => opt.MapFrom(src => src.SubscriptionStartDate.HasValue
                ? src.SubscriptionStartDate.Value.ToString("O")
                : string.Empty))
            .ForMember(x => x.SubscriptionEndDate,
              opt => opt.MapFrom(src => src.SubscriptionEndDate.HasValue
                ? src.SubscriptionEndDate.Value.ToString("O")
                : string.Empty));
          cfg.CreateMap<ImportedFile, ImportedFileDescriptor>()
            .ForMember(x => x.ImportedUtc, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(x => x.LegacyFileId, opt => opt.MapFrom(src => src.ImportedFileId))
            .ForMember(x => x.ImportedFileHistory, opt => opt.MapFrom(src => src.ImportedFileHistory.ImportedFileHistoryItems))
            .ForMember(x => x.IsActivated, opt => opt.UseValue(true));
          cfg.CreateMap<Repositories.DBModels.ImportedFileHistoryItem, Models.ImportedFileHistoryItem>()
            .ForMember(x => x.FileCreatedUtc, opt => opt.MapFrom(src => src.FileCreatedUtc))
            .ForMember(x => x.FileUpdatedUtc, opt => opt.MapFrom(src => src.FileUpdatedUtc));
          cfg.CreateMap<ImportedFile, UpdateImportedFileEvent>()
            .ForMember(x => x.ImportedFileUID, opt => opt.MapFrom(src => Guid.Parse(src.ImportedFileUid)))
            .ForMember(x => x.ProjectUID, opt => opt.MapFrom(src => Guid.Parse(src.ProjectUid)))
            .ForMember(x => x.ActionUTC, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(x => x.ReceivedUTC, opt => opt.MapFrom(src => src.LastActionedUtc));

          // for v2 BC apis
          cfg.CreateMap<Repositories.DBModels.Project, ProjectV2Descriptor>()
            .ForMember(x => x.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(x => x.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")));
          cfg.CreateMap<CreateProjectV2Request, CreateProjectEvent>()
            .ForMember(x => x.CustomerID, opt => opt.Ignore())
            .ForMember(x => x.CustomerUID, opt => opt.Ignore())
            .ForMember(x => x.ProjectBoundary, opt => opt.Ignore())
            .ForMember(x => x.CoordinateSystemFileName, opt => opt.Ignore())
            .ForMember(x => x.CoordinateSystemFileContent, opt => opt.Ignore())
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore())
            .ForMember(x => x.ProjectID, opt => opt.Ignore());
            // todo convert PointLL and CoordSystem
          cfg.CreateMap<Repositories.DBModels.Project, ProjectV2Descriptor>()
            .ForMember(x => x.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(x => x.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")));
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}