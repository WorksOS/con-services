using System;
using AutoMapper;
using CCSS.Geometry;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

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
          cfg.CreateMap<ProjectDatabaseModel, ProjectV6Descriptor>()
            .ForMember(dest => dest.ProjectGeofenceWKT, opt => opt.MapFrom(src => src.Boundary))
            .ForMember(dest => dest.IanaTimeZone, opt => opt.MapFrom(src => src.ProjectTimeZoneIana))
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.MapFrom(src => src.ShortRaptorProjectId))
            .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived));
          cfg.CreateMap<ImportedFile, ImportedFileDescriptor>()
            .ForMember(dest => dest.ImportedUtc, opt => opt.MapFrom(src => src.LastActionedUtc))
            .ForMember(dest => dest.LegacyFileId, opt => opt.MapFrom(src => src.ImportedFileId))
            .ForMember(dest => dest.ImportedFileHistory, opt => opt.MapFrom(src => src.ImportedFileHistory.ImportedFileHistoryItems))
            .ForMember(dest => dest.IsActivated, opt => opt.MapFrom(x => true));
          cfg.CreateMap<Productivity3D.Project.Abstractions.Models.DatabaseModels.ImportedFileHistoryItem, MasterData.Project.WebAPI.Common.Models.ImportedFileHistoryItem>()
            .ForMember(dest => dest.FileCreatedUtc, opt => opt.MapFrom(src => src.FileCreatedUtc))
            .ForMember(dest => dest.FileUpdatedUtc, opt => opt.MapFrom(src => src.FileUpdatedUtc));
          cfg.CreateMap<ImportedFile, UpdateImportedFileEvent>()
            .ForMember(dest => dest.ImportedFileUID, opt => opt.MapFrom(src => Guid.Parse(src.ImportedFileUid)))
            .ForMember(dest => dest.ProjectUID, opt => opt.MapFrom(src => Guid.Parse(src.ProjectUid)))
            .ForMember(dest => dest.ActionUTC, opt => opt.MapFrom(src => src.LastActionedUtc));

          // for v5 TBC apis
          cfg.CreateMap<ProjectDetailResponseModel, ProjectDataTBCSingleResult>()
            .ForMember(dest => dest.LegacyProjectId, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => DateTime.MinValue))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => DateTime.MaxValue))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(x => 0)) // old standard type
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.Message, opt => opt.Ignore());
          
         cfg.CreateMap<TBCPoint, VSS.MasterData.Models.Models.Point>()
            .ForMember(dest => dest.y, opt => opt.MapFrom((src => src.Latitude)))
            .ForMember(dest => dest.x, opt => opt.MapFrom((src => src.Longitude)));
          // ProjectGeofenceAssociations
          cfg.CreateMap<GeofenceWithAssociation, GeofenceV4Descriptor>();

          // cws clients
          cfg.CreateMap<ProjectValidation, CreateProjectRequestModel>()
            .ForMember(dest => dest.TRN, opt => opt.Ignore())
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.CustomerUid))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.Timezone, opt => opt.Ignore())
            .ForMember(dest => dest.Boundary, opt => opt.Ignore()) // done externally
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
            .ForMember(dest => dest.CalibrationFileName, opt => opt.MapFrom(src => src.CoordinateSystemFileName))
            .ForMember(dest => dest.CalibrationFileBase64Content, opt => opt.MapFrom(src => src.CoordinateSystemFileContent))
            ;

          cfg.CreateMap<AccountResponseModel, CustomerData>()
            .ForMember(dest => dest.uid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.type, opt => opt.MapFrom(c => CustomerType.Customer.ToString()))
            ;
          cfg.CreateMap<AccountResponseModel, AccountHierarchyCustomer>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => "Customer"))
            .ForMember(dest => dest.Children, opt => opt.Ignore());

          cfg.CreateMap<ProjectResponseModel, ProjectData>()
            .ForMember(dest => dest.ProjectUID, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.CustomerUID, opt => opt.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.ProjectGeofenceWKT, opt => opt.MapFrom(src => GeometryConversion.ProjectBoundaryToWKT(src.Boundary)))
            .ForMember(dest => dest.IanaTimeZone, opt => opt.MapFrom(src => src.Timezone))
            .ForMember(dest => dest.CoordinateSystemFileName, opt => opt.Ignore())
            .ForMember(dest => dest.CoordinateSystemLastActionedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectTimeZone, opt => opt.Ignore())
            .ForMember(dest => dest.ShortRaptorProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
            .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
            ;

          cfg.CreateMap<ProjectValidateDto, ProjectValidation>()
            .ForMember(dest => dest.CustomerUid, opt => opt.MapFrom(src => TRNHelper.ExtractGuid(src.AccountTrn)))
            .ForMember(dest => dest.ProjectUid, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ProjectTrn) ? null : TRNHelper.ExtractGuid(src.ProjectTrn)))
            .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
            .ForMember(dest => dest.UpdateType, opt => opt.MapFrom(src => ResolveUpdateType(src.UpdateType)))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
            .ForMember(dest => dest.ProjectBoundaryWKT, opt => opt.MapFrom(src => GeometryConversion.ProjectBoundaryToWKT(src.Boundary)))
            .ForMember(dest => dest.CoordinateSystemFileName, opt => opt.MapFrom(src => src.CoordinateSystemFileName))
            .ForMember(dest => dest.CoordinateSystemFileContent, opt => opt.MapFrom(src => src.CoordinateSystemFileContent))
            ;
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }

    private static ProjectUpdateType ResolveUpdateType(CwsUpdateType updateType)
    {
      switch (updateType)
      {
        case CwsUpdateType.CreateProject:
          return ProjectUpdateType.Created;
        case CwsUpdateType.UpdateProject:
          return ProjectUpdateType.Updated;
        case CwsUpdateType.DeleteProject:
          return ProjectUpdateType.Deleted;
        case CwsUpdateType.CalibrationUpdate:
          return ProjectUpdateType.Updated;
        case CwsUpdateType.BoundaryUpdate:
          return ProjectUpdateType.Updated;
        case CwsUpdateType.UpdateProjectStatus:
          //At the moment can only go from Active to Archived
          return ProjectUpdateType.Deleted;
        case CwsUpdateType.ArchiveProject:
          return ProjectUpdateType.Deleted;
        case CwsUpdateType.UpdateProjectType:
          return ProjectUpdateType.Updated;
        default:
          return ProjectUpdateType.None;
      }
    }
  }
}
