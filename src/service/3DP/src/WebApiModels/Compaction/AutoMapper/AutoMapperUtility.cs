using AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
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

    private static IMapper automapper;

    public static IMapper Automapper
    {
      get
      {
        if (automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return automapper;
      }
    }

    public static void ConfigureAutomapper()
    {

      automapperConfiguration = new MapperConfiguration(
        //define mappings <source type, destination type>
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.AddProfile<CmvSettingsProfile>();
          cfg.AddProfile<CmvSettingsExProfile>();
          cfg.AddProfile<MdpSettingsProfile>();
          cfg.AddProfile<TemperatureSettingsProfile>();
          cfg.AddProfile<TemperatureDetailsSettingsProfile>();
          cfg.AddProfile<PassCountSettingsProfile>();
          cfg.AddProfile<CmvPercentChangeSettingsProfile>();
          cfg.AddProfile<CutFillSettingsProfile>();
          cfg.AddProfile<LiftBuildSettingsProfile>();
        }
      );

      automapper = automapperConfiguration.CreateMapper();
    }
  }
}
