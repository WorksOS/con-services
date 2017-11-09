using System;
using AutoMapper;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper
{
  public class AutoMapperUtility
  {
    [ThreadStatic]
    private static MapperConfiguration automapperConfiguration;

    public static MapperConfiguration AutomapperConfiguration
    {
      get
      {
        if (automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return automapperConfiguration;
      }
    }

    private static IMapper _automapper;

    public static IMapper Automapper
    {
      get
      {
        if (automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapper;
      }
    }

    public static void ConfigureAutomapper()
    {
      automapperConfiguration = new MapperConfiguration(
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.AddProfile<FilterProfile>();
          cfg.AddProfile<FilterBoundaryProfile>();
          cfg.AddProfile<ProjectGeofenceProfile>();
        }
      );

      _automapper = automapperConfiguration.CreateMapper();
    }
  }
}