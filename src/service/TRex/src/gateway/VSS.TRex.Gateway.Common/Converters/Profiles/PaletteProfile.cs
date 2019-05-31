using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Rendering.Palettes;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class PaletteProfile : Profile
  {

    public PaletteProfile()
    {
      CreateMap<Transition, ColorPalette>()
        .ForMember(x => x.Value,
          opt => opt.MapFrom(f => f.Value))
        .ForMember(x => x.Color,
          opt => opt.MapFrom(f => 
            (uint)((f.Color.A << 24) | (f.Color.R << 16) |
                   (f.Color.G << 8) | (f.Color.B << 0))));
    }
  }
}
