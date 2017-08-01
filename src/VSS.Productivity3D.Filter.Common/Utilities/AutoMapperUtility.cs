using AutoMapper;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities
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
          cfg.CreateMap<MasterData.Repositories.DBModels.Filter, FilterDescriptor>();
          cfg.CreateMap<FilterRequestFull, CreateFilterEvent>()
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
          cfg.CreateMap<FilterRequestFull, UpdateFilterEvent>()
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
          cfg.CreateMap<FilterRequestFull, DeleteFilterEvent>()
            .ForMember(x => x.ActionUTC, opt => opt.Ignore())
            .ForMember(x => x.ReceivedUTC, opt => opt.Ignore());
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}