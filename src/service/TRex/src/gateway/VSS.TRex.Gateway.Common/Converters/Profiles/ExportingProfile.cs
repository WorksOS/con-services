using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Requests;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class ExportingProfile : Profile
  {
    public ExportingProfile()
    {
      CreateMap<UserPreferences, CSVExportUserPreferences>()
        .ForMember(x => x.ProjectTimeZoneOffset,
          opt => opt.MapFrom(f => f.TimeZoneOffset));

      CreateMap<CompactionVetaExportRequest, CompactionCSVExportRequest>()
        .ForMember(x => x.RestrictOutputSize,
          opt => opt.UseValue(false))
        .ForMember(x => x.RawDataAsDBase,
          opt => opt.UseValue(false));

      CreateMap<CompactionPassCountExportRequest, CompactionCSVExportRequest>()
        .ForMember(x => x.MachineNames,
          opt => opt.UseValue(new string[0]));

      CreateMap<CompactionCSVExportRequest, CSVExportRequestArgument>()
        .ForMember(x => x.ProjectID,
          opt => opt.MapFrom(f => f.ProjectUid))
        .ForMember(x => x.TRexNodeID,
          opt => opt.Ignore())
        .ForMember(x => x.ExternalDescriptor,
          opt => opt.Ignore())
        .ForMember(x => x.Filters,
          opt => opt.Ignore())
        .ForMember(x => x.ReferenceDesignUID,
          opt => opt.Ignore())
        // MappedMachines are mapped separately using CSVExportHelper.MapRequestedMachines()
        .ForMember(x => x.MappedMachines,
          opt => opt.Ignore());
    }
  }
}
