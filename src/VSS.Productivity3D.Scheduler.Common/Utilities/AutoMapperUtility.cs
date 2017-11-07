using AutoMapper;
using VSS.Productivity3D.Scheduler.Common.Models;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
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
          cfg.CreateMap<ProjectImportedFile, NhOpImportedFile>();
          cfg.CreateMap<NhOpImportedFile, ProjectImportedFile>()
            .ForMember(dest => dest.ImportedFileUid, opt => opt.Ignore())
            .ForMember(dest => dest.ImportedFileId, opt => opt.Ignore())
            .ForMember(dest => dest.FileDescriptor, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.UseValue(false))
            .ForMember(dest => dest.IsActivated, opt => opt.UseValue(true));
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}