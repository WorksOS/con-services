using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Rendering.Palettes;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class PaletteProfile : Profile
  {
    public PaletteProfile()
    {
      //Note: Transition is a struct and ColorPalette is a class. 
      //This is the only way I could get AutoMapper to work.
      CreateMap<Transition, ColorPalette>().ConvertUsing(x =>
      {
        var color = (uint) ((x.Color.A << 24) | (x.Color.R << 16) |
                            (x.Color.G << 8) | (x.Color.B << 0));
        return new ColorPalette(color, x.Value);
      });
    }
  }
}
