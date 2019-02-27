using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class PVMDisplayerBase : ProductionPVMDisplayerBase
  {
    public DisplayMode DisplayMode { get; set; } = DisplayMode.Height;

    public IPlanViewPalette Palette { get; set; }

    //        private DisplayPaletteBase palette = null;
    //        public DisplayPaletteBase Palette { get { return palette; } set { SetPalette(value); } }
    //        private virtual void SetPalette(DisplayPaletteBase value) => Palette = value;

    public PVMDisplayerBase()
    {
    }
  }
}
