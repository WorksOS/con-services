using System;
using AutoMapper;
using VSS.Productivity3D.Scheduler.Common.Models;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public class AutoMapperUtility
  {
    [ThreadStatic]
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

    [ThreadStatic]
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
          cfg.CreateMap<ImportedFileProject, ImportedFileNhOp>();
          cfg.CreateMap<ImportedFileNhOp, ImportedFileProject>()
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