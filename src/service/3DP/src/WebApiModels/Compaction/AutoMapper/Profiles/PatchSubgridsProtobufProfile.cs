using AutoMapper;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper.Profiles
{
  public class PatchSubgridsProtobufProfile : Profile
  {
    public PatchSubgridsProtobufProfile()
    {
      // source, dest
      CreateMap<PatchSubgridsRawResult, PatchSubgridsProtobufResult>()
        .ForMember(d => d.Code,
          opt => opt.MapFrom(s => s.Code));

    }
  }
}
