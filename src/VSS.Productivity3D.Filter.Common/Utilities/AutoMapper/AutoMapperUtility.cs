using AutoMapper;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper.Profiles;

namespace VSS.Productivity3D.Filter.Common.Utilities.AutoMapper
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
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.AddProfile<FilterProfile>();
          cfg.AddProfile<FilterBoundaryProfile>();
          cfg.AddProfile<ProjectGeofenceProfile>();
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}