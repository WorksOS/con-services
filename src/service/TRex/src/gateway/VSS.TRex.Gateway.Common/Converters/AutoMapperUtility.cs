using AutoMapper;
using VSS.TRex.Gateway.Common.Converters.Profiles;

namespace VSS.TRex.Gateway.Common.Converters
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
          cfg.AddProfile<BoundingWorldExtent3DProfile>();
          cfg.AddProfile<FenceProfile>();
          cfg.AddProfile<CombinedFilterProfile>();
          cfg.AddProfile<DesignResultProfile>();
          cfg.AddProfile<ReportingProfile>();
          cfg.AddProfile<ExportingProfile>();
          cfg.AddProfile<PointProfile>();
          cfg.AddProfile<MachineProfile>();
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
