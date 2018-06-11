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
            .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.CustomerID ?? 0))
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectID, opt => opt.Ignore());
          cfg.CreateMap<CreateProjectEvent, Repositories.DBModels.Project>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.ProjectStartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.ProjectEndDate))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.GeometryWKT, opt => opt.MapFrom(src => src.ProjectBoundary))
            .ForMember(dest => dest.LegacyProjectID, opt => opt.MapFrom(src => src.ProjectID))
            .ForMember(dest => dest.LegacyCustomerID, opt => opt.MapFrom(src => src.CustomerID))
            .ForMember(dest => dest.LandfillTimeZone, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionUID, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionStartDate, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionEndDate, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceTypeID, opt => opt.Ignore())
            .ForMember(dest => dest.CoordinateSystemLastActionedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.LastActionedUTC, opt => opt.Ignore());
          cfg.CreateMap<UpdateProjectRequest, UpdateProjectEvent>()
            .ForMember(dest => dest.ActionUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectTimezone, opt => opt.Ignore());
          cfg.CreateMap<Repositories.DBModels.Project, ProjectV4Descriptor>()
            .ForMember(dest => dest.ProjectGeofenceWKT, opt => opt.MapFrom(src => src.GeometryWKT))
            .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.ServiceTypeID))
            .ForMember(dest => dest.IanaTimeZone, opt => opt.MapFrom(src => src.LandfillTimeZone))
            .ForMember(dest => dest.IsArchived,
              opt => opt.MapFrom(src => src.IsDeleted || src.SubscriptionEndDate < DateTime.UtcNow))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")))
            .ForMember(dest => dest.SubscriptionStartDate,
              opt => opt.MapFrom(src => src.SubscriptionStartDate.HasValue
                ? src.SubscriptionStartDate.Value.ToString("O")
                : string.Empty))
            .ForMember(dest => dest.SubscriptionEndDate,
              opt => opt.MapFrom(src => src.SubscriptionEndDate.HasValue
                ? src.SubscriptionEndDate.Value.ToString("O")
                : string.Empty));
          cfg.CreateMap<ImportedFile, ImportedFileDescriptor>()
            .ForMember(dest => dest.ImportedUtc, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(dest => dest.LegacyFileId, opt => opt.MapFrom(src => src.ImportedFileId))
            .ForMember(dest => dest.ImportedFileHistory, opt => opt.MapFrom(src => src.ImportedFileHistory.ImportedFileHistoryItems))
            .ForMember(dest => dest.IsActivated, opt => opt.UseValue(true));
          cfg.CreateMap<Repositories.DBModels.ImportedFileHistoryItem, Models.ImportedFileHistoryItem>()
            .ForMember(dest => dest.FileCreatedUtc, opt => opt.MapFrom(src => src.FileCreatedUtc))
            .ForMember(dest => dest.FileUpdatedUtc, opt => opt.MapFrom(src => src.FileUpdatedUtc));
          cfg.CreateMap<ImportedFile, UpdateImportedFileEvent>()
            .ForMember(dest => dest.ImportedFileUID, opt => opt.MapFrom(src => Guid.Parse(src.ImportedFileUid)))
            .ForMember(dest => dest.ProjectUID, opt => opt.MapFrom(src => Guid.Parse(src.ProjectUid)))
            .ForMember(dest => dest.ActionUTC, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(dest => dest.ReceivedUTC, opt => opt.MapFrom(src => src.LastActionedUtc));

          // for v2 BC apis
          cfg.CreateMap<Repositories.DBModels.Project, ProjectV2DescriptorResult>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("O")))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("O")))
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Message, opt => opt.Ignore());
          cfg.CreateMap<CreateProjectV2Request, CreateProjectEvent>()
            .ForMember(dest => dest.CustomerID, opt => opt.UseValue(0))
            .ForMember(dest => dest.CustomerUID, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.ProjectBoundary, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.CoordinateSystemFileName, opt => opt.MapFrom((src => src.CoordinateSystem.Name)))
            .ForMember(dest => dest.CoordinateSystemFileContent, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.ActionUTC, opt => opt.UseValue(DateTime.UtcNow))
            .ForMember(dest => dest.ReceivedUTC, opt => opt.UseValue(DateTime.UtcNow))
            .ForMember(dest => dest.ProjectID, opt => opt.UseValue(0))
            .ForMember(dest => dest.ProjectUID, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore());
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}