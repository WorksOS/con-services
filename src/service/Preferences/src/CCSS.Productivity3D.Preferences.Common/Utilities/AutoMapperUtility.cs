using AutoMapper;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using PrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.PreferenceKey;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;


namespace CSS.Productivity3D.Preferences.Common.Utilities
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
          
          cfg.CreateMap<UserPrefKeyDataModel, UserPreferenceV1Result>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(x => ContractExecutionStatesEnum.ExecutedSuccessfully))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(x => ContractExecutionResult.DefaultMessage))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.KeyName))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => src.PreferenceKeyUID))
            .ForMember(dest => dest.PreferenceJson, opt => opt.MapFrom(src => src.PreferenceJson))
            .ForMember(dest => dest.SchemaVersion, opt => opt.MapFrom(src => src.SchemaVersion));
        

          cfg.CreateMap<PrefKeyDataModel, PreferenceKeyV1Result>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(x => ContractExecutionStatesEnum.ExecutedSuccessfully))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(x => ContractExecutionResult.DefaultMessage))
            .ForMember(dest => dest.PreferenceKeyName, opt => opt.MapFrom(src => src.KeyName))
            .ForMember(dest => dest.PreferenceKeyUID, opt => opt.MapFrom(src => src.PreferenceKeyUID));
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();
    }
  }
}
