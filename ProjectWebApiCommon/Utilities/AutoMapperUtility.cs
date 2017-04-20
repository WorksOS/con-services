using AutoMapper;
using ProjectWebApi.Models;
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
        cfg => cfg.CreateMap<CreateProjectRequest, CreateProjectEvent>().ForMember(x => x.ActionUTC, opt => opt.Ignore()).ForMember(x => x.ReceivedUTC, opt => opt.Ignore()));

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
